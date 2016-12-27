module Claims

open Suave
open System.Security.Claims
open System

let UserIdKey = "userId"

let inline getCustomClaims (user : ^a when ^a:(member GetName: unit -> string) and
                                           ^a:(member GetId: unit -> string)) =     
    let username = (^a : (member GetName : unit -> string) user)
    let userId = (^a : (member GetId : unit -> string) user)
    [(Suave.Authentication.UserNameKey, username); (UserIdKey, userId)]

let addClaim (claim : string * string) (identity : ClaimsIdentity) =
    identity.AddClaim(new Claim(fst claim, snd claim))
    
let getUserIdFromContext (context : HttpContext) =
     unbox(context.userState.[UserIdKey]) |> Guid.Parse

