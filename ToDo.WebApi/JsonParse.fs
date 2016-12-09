module JsonParse
   open Chiron
   open Chiron.Mapping
   open Chiron.Operators
   open System.Text
   open Infrastructure

   open Business.createToDoCommand
   
   let inline private deserializeJson builder bytes  =
        bytes |> Encoding.ASCII.GetString 
              |> Json.parse
              |> builder
              |> function | Value r,_ -> Railroad.Success r 
                          | Error e,_ -> Railroad.Error(Sentences.Validation.invalidJson, [e])

   module ToDo =        
        let private deserializeCreateToDoCommand' =            
            json {
                let! id = Json.read "id"
                let! description = Json.read "description"
                let! creatorId = Json.read "creatorId"
                let! tagsIds = Json.read "tagsIds"
                return { id = id; description = description; creatorId = creatorId; tagsIds = tagsIds }
            }

        let deserializeCreateToDoCommand = deserializeJson deserializeCreateToDoCommand'

