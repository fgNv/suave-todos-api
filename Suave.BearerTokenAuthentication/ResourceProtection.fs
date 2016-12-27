module ResourceProtection

open Microsoft.Owin.Security
open Microsoft.Owin.Security.DataHandler
open Microsoft.Owin.Security.DataProtection
open Suave
open Owin
open Microsoft.Owin.Builder
open Microsoft.Owin.Security.OAuth
open Microsoft.Owin.Security.Infrastructure
open Railroad

let private hostAppName = "bearerTokenAuthentication"

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
            Error("invalidToken", [|"tokenWithoutExpirationDate"|])
        | Some expirationUtc when expirationUtc < currentUtc ->
            Error("invalidToken", [|"tokenExpired"|])
        | Some expirationUtc -> Success ticket
            
let private executeVerifications authorizationHeaderContent =                         
    authorizationHeaderContent |> 
    extractToken >>=
    validateToken >>=
    verifyTokenExpiration
    
let private getAuthorizationHeaderFromContext ctx =        
    match ctx.request.header "Authorization" with
          | Choice1Of2 header -> Success header
          | Choice2Of2 _ -> Error ("authenticationFailure", 
                                   [| "noAuthenticationHeaderFound" |])

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