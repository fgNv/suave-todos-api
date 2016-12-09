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
    open JsonParse.ToDo
    
    let inline private executeCommand deserializeCommand handleCommand request  =
        let result = request.rawForm |> deserializeCommand >>= handleCommand
        match result with
              | Success i -> OK "deu tudo certo"
              | Error (title, errors) ->  BAD_REQUEST (title + " - nem deu")

    let apiRoutes = 
        choose [ path "/user" >=> 
                       choose [ GET >=> OK "user getzera"
                                POST >=> OK "user postzera" ] 
                 path "/todo" >=> 
                       choose [ GET >=> OK "todo getzera"
                                POST >=> request 
                                    (executeCommand 
                                        deserializeCreateToDoCommand Application.ToDo.createToDo) ]
                 GET >=> NOT_FOUND "kdddd" ]
