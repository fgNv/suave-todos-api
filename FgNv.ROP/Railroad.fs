﻿module Railroad

open System

type ErrorContent =
    | Title of string
    | TitleMessages of string * seq<string>

type Result<'TEntity> =  
    | Success of 'TEntity
    | Error of string * seq<string>
    
let isError result =
    match result with | Success _ -> false | Error _ -> true

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
