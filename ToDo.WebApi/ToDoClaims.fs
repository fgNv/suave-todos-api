module ToDoClaims

open Suave
open System

let UserIdKey = "userId"

let inline getCustomClaims (user : Business.User) =     
    [(Suave.Authentication.UserNameKey, user.name); (UserIdKey, user.id.ToString())]

let getUserIdFromContext (context : HttpContext) =
     unbox(context.userState.[UserIdKey]) |> Guid.Parse

