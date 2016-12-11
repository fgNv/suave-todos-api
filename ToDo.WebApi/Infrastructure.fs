﻿module Infrastructure

open System

    module Railroad =
        type Result<'TEntity> =  
            | Success of 'TEntity
            | Error of string * seq<string>

        let bind switchFunction input =
            match input with
                | Success result -> switchFunction result
                | Error (description, errors) -> Error (description, errors)

        let (>>=) input switchFunction =
            bind switchFunction input

    module Error =
        let rec private getExceptionMessages' (ex : System.Exception, result : List<string>) =
            match ex.InnerException with 
                | null -> ex.Message :: result
                | inner -> getExceptionMessages'(inner, ex.Message :: result)

        let getExceptionMessages (ex : System.Exception) =
            getExceptionMessages'(ex, [])
