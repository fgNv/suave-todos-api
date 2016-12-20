module JsonParse

open Chiron
open Chiron.Mapping
open Chiron.Operators
open System.Text
open System
open Infrastructure

let private onNoneGenerateGuid input =
    match input with
        | Some guid -> guid
        | None -> Guid.NewGuid()

let inline private deserializeJson builder bytes  =
     bytes |> Encoding.ASCII.GetString 
           |> Json.parse
           |> builder
           |> function | Value r,_ -> Railroad.Success r 
                       | Error e,_ -> Railroad.Error(Sentences.Validation.invalidJson, [e])

module User =
    open Business.createUserCommand

    let private deserializeCreateUserCommand' =            
         json {
             let! id = Json.tryRead "id"
             let! name = Json.read "name"
             let! password = Json.read "password"
             return { id = onNoneGenerateGuid id; name = name; password = password }
         }

    let deserializeCreateUserCommand = deserializeJson deserializeCreateUserCommand'

module Tag =
    open Business.createTagCommand

    let private deserializeCreateTagCommand' creatorId =            
         json {
             let! id = (Json.tryRead "id")
             let! name = Json.read "name"
             return { id = onNoneGenerateGuid id; name = name; creatorId = creatorId }
         }

    let deserializeCreateTagCommand creatorId = deserializeJson (deserializeCreateTagCommand' creatorId)

module ToDo =        
    open Business.createToDoCommand

    let private deserializeCreateToDoCommand' =            
         json {
             let! id = Json.tryRead "id"
             let! description = Json.read "description"
             let! creatorId = Json.read "creatorId"
             let! tagsIds = Json.read "tagsIds"
             return { id = onNoneGenerateGuid id; description = description; creatorId = creatorId; tagsIds = tagsIds }
         }

    let deserializeCreateToDoCommand = deserializeJson deserializeCreateToDoCommand'
