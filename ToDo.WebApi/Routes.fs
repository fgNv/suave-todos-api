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

let inline private executeCommand deserializeCommand handleCommand (request : HttpRequest) =
    let result = request.rawForm |> deserializeCommand >>= handleCommand

    match result with
          | Success i -> OK "deu tudo certo"
          | Error (title, errors) -> BAD_REQUEST (title + " - nem deu")

let inline private executeCommand' deserializeCommand handleCommand (ctx : HttpContext) =
    let result = ctx.request.rawForm |> deserializeCommand ctx >>= handleCommand
    
    match result with
          | Success i -> OK "deu tudo certo"
          | Error (title, errors) -> BAD_REQUEST (title + " - nem deu")

let apiRoutes =
    let protectResourceClaimList = 
        protectResource [|Suave.Authentication.UserNameKey; ToDoClaims.UserIdKey|]

    let retrieveToken = AuthorizationServer.authorizationServerMiddleware 
                                PgSqlPersistence.User.validateCredentials
                                ToDoClaims.getCustomClaims

    let desCreateTagCmd ctx = 
        JsonParse.Tag.deserializeCreateTagCommand (ToDoClaims.getUserIdFromContext ctx)

    let extractDataFromContext key = 
        context( fun ctx -> unbox(ctx.userState.[key]))

    let tagResource = choose [ GET  >=> OK "tag getzera"
                               POST >=>               
                                    context(executeCommand' desCreateTagCmd Application.Tag.createTag) ]

    choose [ path "/token" >=> retrieveToken
             path "/user" >=> 
                   choose [ GET >=> OK "user getzera"
                            POST >=> request 
                                (executeCommand 
                                    JsonParse.User.deserializeCreateUserCommand Application.User.createUser) ] 
             protectResourceClaimList ( path "/tag" >=> tagResource)
             path "/todo" >=> 
                   choose [ GET >=> OK "todo getzera"
                            POST >=> request 
                                (executeCommand 
                                    JsonParse.ToDo.deserializeCreateToDoCommand Application.ToDo.createToDo) ]
             GET >=> NOT_FOUND "kdddd" ]