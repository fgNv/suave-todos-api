module OwinAuthentication

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

type private SimpleAuthenticationProvider(validateUserCredentials) =
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
                    identity.AddClaim(new Claim("username", user))
                    context.Validated(identity) |> ignore
                | Error (title, errors) -> 
                    context.SetError(Sentences.Error.authenticationFailure, Sentences.Validation.invalidCredentials)
        }
        upcast Async.StartAsTask f 

let private hostAppName = "ToDoApi"

let authorizationServerMiddleware validateUserCredentials =
    let serverOptions = new OAuthAuthorizationServerOptions(
                            AllowInsecureHttp = true,
                            TokenEndpointPath= new PathString("/token"),
                            AccessTokenExpireTimeSpan = TimeSpan.FromDays(1.0),
                            Provider = new SimpleAuthenticationProvider(validateUserCredentials) )
    
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
                                           
    let private getUseNameFromClaims (ticket : AuthenticationTicket) =
        let claim = ticket.Identity.Claims |> Seq.tryFind(fun c -> c.Type = "username")
        match claim with
            | Some c -> c.Value
            | None -> "i lesq"

    let inline private addUserName username ctx = { 
        ctx with userState = ctx.userState |> Map.add Suave.Authentication.UserNameKey (box username) }

    let protectResource (protectedPart : WebPart) (ctx : HttpContext) =     
        
        let result = ctx |> getAuthorizationHeaderFromContext >>= executeVerifications
        
        match result with
        | Success authTicket ->                 
            protectedPart (addUserName (getUseNameFromClaims authTicket) ctx)
        | Error(title, messages) -> 
            Suave.RequestErrors.challenge ctx
