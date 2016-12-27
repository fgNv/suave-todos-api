module Infrastructure

open System
open System.Text
open System.Security.Cryptography

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
