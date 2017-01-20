module Routes

open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Application
open System
open Railroad
open ResourceProtection
open JsonParse

let inline private executeCommand deserializeCommand handleCommand (request : HttpRequest) =
    let result = request.rawForm |> deserializeCommand >>= handleCommand

    match result with
          | Success i -> OK "deu tudo certo"
          | Error (title, errors) -> BAD_REQUEST (title + " - nem deu")
          
let apiRoutes =
    let protectResource' = 
        protectResource [|Suave.Authentication.UserNameKey; ToDoClaims.UserIdKey|]
                                
    choose [ path "/token" >=> AuthorizationServer.authorizationServerMiddleware 
                               PgSqlPersistence.User.validateCredentials
                               ToDoClaims.getCustomClaims
             path "/user" >=> 
                   choose [ GET >=> OK "user get"
                            POST >=> request 
                                (executeCommand CreateUserCommand.deserialize User.createUser) ] 
             path "/tag" >=> protectResource' (
                   choose [ GET  >=> OK "tag get"
                            POST >=> context(fun ctx ->   
                                let userId = ToDoClaims.getUserIdFromContext ctx
                                request(executeCommand (CreateTagCommand.deserialize userId) Tag.createTag)
                            )])
             path "/todo" >=> 
                   choose [ GET >=> OK "todo get"
                            POST >=> request 
                                (executeCommand CreateToDoCommand.deserialize ToDo.createToDo) ]
             GET >=> NOT_FOUND "no resource matches the request" ]