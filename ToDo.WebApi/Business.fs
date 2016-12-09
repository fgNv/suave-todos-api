module Business
    open System
    open Infrastructure.Railroad    
    
    let inline private validate getErrors input =
        let errors = getErrors input

        match errors |> Seq.isEmpty with
                | true -> Result.Success input
                | false -> Result.Error (Sentences.Validation.validationFailed, errors)

    module createTagsCommand =
        type item = {id: Guid; name: string}
        type command = {items: item seq }

        let private validateTag item =
            seq { if item.id = Guid.Empty then
                    yield Sentences.Validation.idIsRequired
                  if String.IsNullOrWhiteSpace item.name then
                    yield Sentences.Validation.nameIsRequired }

        let private getErrors command =
            command.items |> Seq.collect validateTag 
                          |> Seq.distinct
            
        let validateTags =
            validate getErrors
            
        let handle saveTags command =
            command |> validateTags >>= saveTags

    module createToDoCommand =           
        type command = { id: Guid
                         description: string 
                         creatorId: Guid
                         tagsIds: Guid list } 

        let private getErrors userExists command =
            seq { if command.id = Guid.Empty then
                     yield Sentences.Validation.idIsRequired              
                  if String.IsNullOrWhiteSpace command.description then
                     yield Sentences.Validation.descriptionIsRequired 
                  if not(userExists command.creatorId) then
                     yield Sentences.Validation.invalidUserId }
        
        let validateToDo userExists = validate (getErrors userExists)

        let handle userExists saveToDo command =
            command |> validateToDo userExists >>= saveToDo
