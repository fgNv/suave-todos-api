module Business
    open System
    open Infrastructure.Railroad    
    
    let inline private validate getErrors input =
        let errors = getErrors input

        match errors |> Seq.isEmpty with
                | true -> Result.Success input
                | false -> Result.Error (Sentences.Validation.validationFailed, errors)

    module createUserCommand =
        type command = {id: Guid; name: string}

        let private getErrors item =
            seq { if item.id = Guid.Empty then
                    yield Sentences.Validation.idIsRequired
                  if String.IsNullOrWhiteSpace item.name then
                    yield Sentences.Validation.nameIsRequired }
            
        let handle createUser command =
            command |> validate getErrors >>= createUser

    module createTagCommand =
        type command = {id: Guid; name: string; creatorId: Guid}

        let private getErrors userExists item =
            seq { if item.id = Guid.Empty then
                    yield Sentences.Validation.idIsRequired
                  if item.creatorId = Guid.Empty then
                    yield Sentences.Validation.cretorIdIsRequired
                  if String.IsNullOrWhiteSpace item.name then
                    yield Sentences.Validation.nameIsRequired
                  if not(userExists item.creatorId) then
                     yield Sentences.Validation.invalidUserId }
                        
        let handle userExists createTag command =
            command |> validate (getErrors userExists) >>= createTag

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
        
        let handle userExists saveToDo command =
            command |> validate (getErrors userExists) >>= saveToDo
