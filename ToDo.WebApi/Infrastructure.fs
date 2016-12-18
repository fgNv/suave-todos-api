module Infrastructure

open System
open System.Text
open System.Security.Cryptography

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

        let errorOnEmptySeq seq =
            match seq |> Seq.isEmpty with
                | true -> Error ("nein", [|"nein"|]) | false -> Success seq

        let inline errorOnNone input =
            match input with 
                | Some v -> Success v
                | None -> Error("nein", [|"nein"|])

        let errorOnEmptyString string =
            match string |> String.IsNullOrWhiteSpace with
                | true -> Error ("nein", [|"nein"|]) | false -> Success string

        type RailroadBuilder() =
            member this.Bind(m, f) = bind f m
            member this.Return (v) = Success v

        let rBuilder = new RailroadBuilder()

    module Cryptography =
        let encrypt (content : string) =
            let shaM = new SHA256Managed()
            content |> UTF8Encoding.UTF8.GetBytes |> shaM.ComputeHash |> UTF8Encoding.UTF8.GetString

    module Error =
        let rec private getExceptionMessages' (ex : System.Exception, result : List<string>) =
            match ex.InnerException with 
                | null -> ex.Message :: result
                | inner -> getExceptionMessages'(inner, ex.Message :: result)

        let getExceptionMessages (ex : System.Exception) =
            getExceptionMessages'(ex, [])
