module Business
    open System
    open Railroad    
    
    let inline private validate getErrors input =
        let errors = getErrors input

        match errors |> Seq.isEmpty with
                | true -> Result.Success input
                | false -> Result.Error (Sentences.Validation.validationFailed, errors)

    type User = {id: Guid; name: string; } 
                member x.GetName() = x.name
                member x.GetId() = x.id.ToString()
    
    module CreateUser =
        type Command = {id: Guid; name: string; password: string}

        let private getErrors item =
            seq { if item.id = Guid.Empty then
                    yield Sentences.Validation.idIsRequired
                  if String.IsNullOrWhiteSpace item.name then
                    yield Sentences.Validation.nameIsRequired
                  if String.IsNullOrWhiteSpace item.password then
                    yield Sentences.Validation.passwordIsRequired }
            
        let handle createUser command =
            command |> validate getErrors >>= createUser

    module CreateTag =
        type Command = {id: Guid; name: string; creatorId: Guid}

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

    module CreateToDo =           
        type Command = { id: Guid
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
