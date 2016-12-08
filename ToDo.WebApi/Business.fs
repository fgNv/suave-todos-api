module Business
    open System
    open Infrastructure.Railroad

    type User = { id: Guid; name : string}
    type Tag = { id: Guid; description: string}

    let inline private validate getErrors input =
        let errors = getErrors input

        match errors |> Seq.isEmpty with
                | true -> Result.Success input
                | false -> Result.Error (Sentences.Validation.validationFailed, errors)

    module createToDoCommand =
        type command = { id: Guid
                         description: string 
                         creatorId: Guid
                         tagsIds: seq<Guid> }

        let private getErrors command =
            seq { if command.id = Guid.Empty then
                     yield Sentences.Validation.idIsRequired              
                  if String.IsNullOrWhiteSpace command.description then
                     yield Sentences.Validation.descriptionIsRequired }
        
        let private validate' = validate getErrors

        let handle saveToDo command =
            command |> validate' >>= saveToDo
