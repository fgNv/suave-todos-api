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
    let app = Routes.apiRoutes

    startWebServer defaultConfig app

    0 
