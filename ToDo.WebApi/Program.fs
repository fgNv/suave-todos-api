open Suave
open Application
open System

[<EntryPoint>]
let main argv = 
    
    let app = Routes.apiRoutes
    startWebServer defaultConfig app
    0 
