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
    let securedPath path = 
        Suave.Owin.OwinApp.ofMidFunc path OwinAuthentication.basicAuthMidFunc

    choose [ path "/user" >=> 
                   choose [ GET >=> OK "user getzera"
                            POST >=> OK "user postzera" ] 
             path "/tag" >=> 
                   choose [ GET >=> OK "user getzera"
                            POST >=> OK "tag postzera" ] 
             securedPath "/todo" >=> 
                   choose [ GET >=> OK "todo getzera"
                            POST >=> request 
                                (executeCommand 
                                    JsonParse.ToDo.deserializeCreateToDoCommand Application.ToDo.createToDo) ]
             GET >=> NOT_FOUND "kdddd" ]
