module OwinAuthentication

open System
open Suave.Owin
open System.Collections.Generic
open System.Text
open System.Threading.Tasks
open Infrastructure.Railroad

type Token = 
    { ExpireIn : DateTime
      AccessToken : string }

let validateCredentials (credentials : string) = 
    match credentials.Split([| ':' |]) with
         | [| "foo"; "bar" |] -> 
            Success credentials
         | _ -> 
            Error(Sentences.Error.authenticationFailure, [| Sentences.Validation.invalidCredentials |])

let private getCredentialsFromAuthenticationHeader (authorizationHeader : string []) = 
    if authorizationHeader |> Seq.isEmpty then
        Error(Sentences.Error.authenticationFailure, 
              [| Sentences.Validation.noAuthenticationHeaderFound |]) 
    else
        match authorizationHeader.[0].Split([| ' ' |]) |> Seq.first with    
        | Some content -> 
            Success(content
                    |> Convert.FromBase64String
                    |> Encoding.UTF8.GetString)
        | None -> Error(Sentences.Error.authenticationFailure, 
                        [| Sentences.Validation.noAuthenticationHeaderFound |])

let private getAuthorizationHeaderFromRequest (env : OwinEnvironment) = 
    let requestHeaders : IDictionary<string, string []> = unbox env.[OwinConstants.requestHeaders]
    let authorizationKey = "Authorization"
    match requestHeaders.ContainsKey authorizationKey with
    | true -> Success requestHeaders.[authorizationKey]
    | false -> Error(Sentences.Error.authenticationFailure, [| Sentences.Validation.noAuthenticationHeaderFound |])

let basicAuthMidFunc = 
    OwinMidFunc(fun next -> 
        OwinAppFunc(fun env -> 
            let result = env |> getAuthorizationHeaderFromRequest >>=
                                getCredentialsFromAuthenticationHeader >>= 
                                validateCredentials 
            
            match result with
            | Success r ->                 
                let task = next.Invoke(env)
                task
            | Error(title, messages) -> 
                env.[OwinConstants.responseStatusCode] <- box 401
                env.[OwinConstants.responseReasonPhrase] <- box "Unauthorized"
                let responseStream : IO.Stream = unbox env.[OwinConstants.responseBody]
                responseStream.Write([||], 0, 0)
                Task.FromResult() :> Task))
