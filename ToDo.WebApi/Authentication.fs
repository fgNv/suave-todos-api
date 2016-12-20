module Authentication

open System
open Suave.Owin
open System.Collections.Generic
open System.Text
open System.Threading.Tasks
open Infrastructure.Railroad
open Microsoft.Owin.Security.OAuth
open Microsoft.Owin.Extensions
open Microsoft.Owin
open System.Security.Claims
open Owin
open Microsoft.Owin.Builder
open Microsoft.Owin.Security.Infrastructure

module Claims =
    open Suave
    
    let UserIdKey = "userId"
    
    let getCustomClaims (user : Business.User) = 
        [(Suave.Authentication.UserNameKey, user.name); (UserIdKey, user.id.ToString())]
    
    let addClaim (claim : string * string) (identity : ClaimsIdentity) =
        identity.AddClaim(new Claim(fst claim, snd claim))
        
    let getUserIdFromContext (context : HttpContext) =
         unbox(context.userState.[UserIdKey]) |> Guid.Parse

type private SimpleAuthenticationProvider<'a>(validateUserCredentials, 
                                              getCustomClaims : 'a -> (string * string) list) =
    inherit OAuthAuthorizationServerProvider()
    override this.ValidateClientAuthentication (context : OAuthValidateClientAuthenticationContext) =
        let f: Async<unit> = async { context.Validated() |> ignore }
        upcast Async.StartAsTask f 

    override this.GrantResourceOwnerCredentials(context: OAuthGrantResourceOwnerCredentialsContext) =        
        let f: Async<unit> = async {  
            let result = validateUserCredentials context.UserName context.Password
            match result with 
                | Success user -> 
                    let identity = new ClaimsIdentity(context.Options.AuthenticationType)
                    identity.AddClaim(new Claim("sub", context.UserName))
                    identity.AddClaim(new Claim("role", "user"))

                    getCustomClaims user |> List.iter(fun tp -> Claims.addClaim tp identity )
                    
                    context.Validated(identity) |> ignore
                | Error (title, errors) -> 
                    context.SetError(Sentences.Error.authenticationFailure, Sentences.Validation.invalidCredentials)
        }
        upcast Async.StartAsTask f 

let private hostAppName = "ToDoApi"

let authorizationServerMiddleware validateUserCredentials getCustomClaims =
    let serverOptions = new OAuthAuthorizationServerOptions(
                            AllowInsecureHttp = true,
                            TokenEndpointPath= new PathString("/token"),
                            AccessTokenExpireTimeSpan = TimeSpan.FromDays(1.0),
                            Provider = new SimpleAuthenticationProvider<'a>(validateUserCredentials, getCustomClaims) )
    
    let builder = new AppBuilder() :> IAppBuilder
    builder.UseOAuthAuthorizationServer(serverOptions) |> ignore
    builder.Properties.["host.AppName"] <- hostAppName
    let owinApp = builder.Build()
    OwinApp.ofAppFunc "/" owinApp

module routeProtection =
    open Microsoft.Owin.Security
    open Microsoft.Owin.Security.DataHandler
    open Microsoft.Owin.Security.DataProtection
    open Suave

    let private buildDefaultBearerOptions ()= 
        let app = new AppBuilder() :> IAppBuilder        
        app.Properties.["host.AppName"] <- hostAppName

        let typeDef = typedefof<OAuthAuthorizationServerMiddleware>
        let defaultDataProtector = app.CreateDataProtector(typeDef.Namespace, "Access_Token", "v1")
        
        let defaultAccessTokenFormat = new TicketDataFormat(defaultDataProtector)
        let defaultOptions = new OAuthBearerAuthenticationOptions(
                                            AccessTokenProvider = new AuthenticationTokenProvider(),
                                            AccessTokenFormat = defaultAccessTokenFormat,
                                            Provider = new OAuthBearerAuthenticationProvider() )        
        app.UseOAuthBearerAuthentication(defaultOptions) |> ignore
        defaultOptions

    let private defaultOptions = buildDefaultBearerOptions()

    let private validateToken requestToken =        
        let unprotectedTicket = defaultOptions.AccessTokenFormat.Unprotect(requestToken)
        Option.ofObj unprotectedTicket |> errorOnNone
        
    let private extractToken (content : string) =
        let startLength = "Bearer ".Length
        let requestToken = content.Substring(startLength).Trim()
        let requestTokenContext = new OAuthRequestTokenContext(null, requestToken);
        errorOnEmptyString requestTokenContext.Token

    let private verifyTokenExpiration (ticket : AuthenticationTicket) =
        let currentUtc = defaultOptions.SystemClock.UtcNow
        match Option.ofNullable ticket.Properties.ExpiresUtc with
            | None -> 
                Error(Sentences.Error.invalidToken, [|Sentences.Error.tokenWithoutExpirationDate|])
            | Some expirationUtc when expirationUtc < currentUtc ->
                Error(Sentences.Error.invalidToken, [|Sentences.Error.tokenExpired|])
            | Some expirationUtc -> Success ticket
                
    let private executeVerifications authorizationHeaderContent =                         
        authorizationHeaderContent |> 
        extractToken >>=
        validateToken >>=
        verifyTokenExpiration
        
    let private getAuthorizationHeaderFromContext ctx =        
        match ctx.request.header "Authorization" with
              | Choice1Of2 header -> Success header
              | Choice2Of2 _ -> Error (Sentences.Error.authenticationFailure, 
                                       [| Sentences.Validation.noAuthenticationHeaderFound |])

    let private getClaim (key : string) (ticket : AuthenticationTicket) =
        let claim = ticket.Identity.Claims |> Seq.tryFind(fun c -> c.Type = key)
        match claim with 
            | Some c -> Success c.Value 
            | None -> Error ("sem claim", [|"CLAIMLESS"|])
                    
    let private getUserNameFromClaims = getClaim Suave.Authentication.UserNameKey
    let private getUserIdFromClaims = getClaim Claims.UserIdKey

    let inline private addClaims claims ctx = 
        let userNameResult = getUserNameFromClaims claims
        let userIdResult = getUserIdFromClaims claims
        match userNameResult, userIdResult with
            | Success userName, Success userId ->
                   Success { ctx with userState = ctx.userState 
                                    |> Map.add Suave.Authentication.UserNameKey (box (userName))
                                    |> Map.add Claims.UserIdKey (box (userId)) }          
            | Error (t1, e1) , _ -> Error (t1, e1)
            | _ , Error (t1, e1) -> Error (t1, e1)

    let protectResource (protectedPart : WebPart) (ctx : HttpContext) =     
        
        let result = ctx |> getAuthorizationHeaderFromContext >>= executeVerifications
        
        match result with
        | Success authTicket ->                         
            let contextWithClaims = addClaims authTicket ctx
            match contextWithClaims with
                | Success context -> protectedPart context
                | Error (t1, e1) -> Suave.RequestErrors.challenge ctx
        | Error(title, messages) -> 
            Suave.RequestErrors.challenge ctx
