open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful
open Suave.RequestErrors
open Application
open System
open Infrastructure.Railroad

[<EntryPoint>]
let main argv = 
    let app = choose 
                    [ path "/user" >=> 
                            choose [ GET >=> OK "user getzera"
                                     POST >=> 
                                        OK "user postzera" ] 
                      path "/todo" >=> 
                            choose [ GET >=> OK "todo getzera"
                                     POST >=>                                         
                                        let r = Application.ToDo.createToDo 
                                                    { id = Guid.NewGuid()
                                                      description = "hue"
                                                      creatorId = Guid.NewGuid()
                                                      tagsIds = [| Guid.NewGuid(); Guid.NewGuid()|] }
                                        match r with
                                            | Success i -> OK "deu tudo certo"
                                            | Error (title, errors) -> OK "nem deu" ]
                      GET >=> NOT_FOUND "kdddd" ]

    startWebServer defaultConfig app

    0 
