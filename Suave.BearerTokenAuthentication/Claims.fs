module Claims

open Suave
open System.Security.Claims
open System

let addClaim (claim : string * string) (identity : ClaimsIdentity) =
    identity.AddClaim(new Claim(fst claim, snd claim))
