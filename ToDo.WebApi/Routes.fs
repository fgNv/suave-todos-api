module Routes

open Suave
open Suave.Http
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Application
open System
open Infrastructure.Railroad

let inline private executeCommand deserializeCommand handleCommand request =
    let result = request.rawForm |> deserializeCommand >>= handleCommand
    
    match result with
          | Success i -> OK "deu tudo certo"
          | Error (title, errors) ->  BAD_REQUEST (title + " - nem deu")

let apiRoutes =    
    let protectResource = OwinAuthentication.routeProtection.protectResource
    let retrieveToken = OwinAuthentication.authorizationServerMiddleware 
                                PgSqlPersistence.User.validateCredentials

    let tagResource = choose [ GET  >=> OK "tag getzera"
                               POST >=> request 
                                  (executeCommand 
                                      JsonParse.Tag.deserializeCreateTagCommand Application.Tag.createTag) ]

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