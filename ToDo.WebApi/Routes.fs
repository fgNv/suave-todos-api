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

let inline private executeCommand deserializeCommand handleCommand (request : HttpRequest) =
    let result = request.rawForm |> deserializeCommand >>= handleCommand
    
    match result with
          | Success i -> OK "deu tudo certo"
          | Error (title, errors) ->  BAD_REQUEST (title + " - nem deu")

let apiRoutes =    
    let protectResource = ResourceProtection.protectResource
    let retrieveToken = AuthorizationServer.authorizationServerMiddleware 
                                PgSqlPersistence.User.validateCredentials
                                Claims.getCustomClaims

    let desCreateTagCmd ctx = 
        JsonParse.Tag.deserializeCreateTagCommand (Claims.getUserIdFromContext ctx)

    let tagResource = choose [ GET  >=> OK "tag getzera"
                               POST >=> 
                                 context(fun ctx -> 
                                    (executeCommand 
                                        (desCreateTagCmd ctx) Application.Tag.createTag ctx.request)) ]

    choose [ path "/token" >=> retrieveToken
             path "/user" >=> 
                   choose [ GET >=> OK "user getzera"
                            POST >=> request 
                                (executeCommand 
                                    JsonParse.User.deserializeCreateUserCommand Application.User.createUser) ] 
             protectResource ( path "/tag" >=> tagResource)
             path "/todo" >=> 
                   choose [ GET >=> OK "todo getzera"
                            POST >=> request 
                                (executeCommand 
                                    JsonParse.ToDo.deserializeCreateToDoCommand Application.ToDo.createToDo) ]
             GET >=> NOT_FOUND "kdddd" ]