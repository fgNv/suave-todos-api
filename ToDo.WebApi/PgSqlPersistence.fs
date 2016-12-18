module PgSqlPersistence
    open FSharp.Data.Sql
    open System.IO
    open Business
    open Infrastructure.Railroad

    [<Literal>]
    let private connectionString = @"User ID=homestead;Password=secret;
                                     Host=192.168.36.36;Port=5432;Database=ingresso_2;"

    [<Literal>]
    let private resolutionPath = "..\\..\\..\\packages\\Npgsql.3.1.9\\lib\\net451\\Npgsql.dll"

    type private pgsqlAccess = SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL,
                                               connectionString,
                                               ResolutionPath = resolutionPath>

    let private getContext() =
        pgsqlAccess.GetDataContext()
        
    module Tag = 
        let insertTag (command: createTagCommand.command) =            
            try 
                let context = getContext()
                let tag = context.Public.Tag.Create()
                tag.Id <- command.id
                tag.Name <- command.name
                tag.CreatorId <- command.creatorId
                context.SubmitUpdates()
                Success command
            with 
                | ex -> Error (Sentences.Error.databaseFailure, 
                               Infrastructure.Error.getExceptionMessages ex)

    module User =
        let userExists id =
            let context = getContext()
            context.Public.User |> Seq.exists(fun u -> u.Id = id)

        let validateCredentials username password =
            let context = getContext()
            
            let encryptedPassword = Infrastructure.Cryptography.encrypt password
            let queryResult = context.Public.User |> 
                                    Seq.tryFind(fun u -> u.Name = username && 
                                                         u.Password = encryptedPassword)
            match queryResult with 
                | Some user -> Success username
                | None -> Error (Sentences.Error.authenticationFailure,
                                 [Sentences.Error.authenticationFailure])

        let insertUser (command: createUserCommand.command) =
            try
                let context = getContext()
                let user = context.Public.User.Create()
                user.Id <- command.id
                user.Name <- command.name
                user.Password <- Infrastructure.Cryptography.encrypt command.password
                context.SubmitUpdates()
                Success command
            with
                | ex -> Error(Sentences.Error.databaseFailure, 
                              Infrastructure.Error.getExceptionMessages ex)
    
    module ToDo =
        let private linkToDoAndTag (context : pgsqlAccess.dataContext) toDoId tagId =
            let toDoTagLink = context.Public.ToDoTags.Create()
            toDoTagLink.TagId <- tagId
            toDoTagLink.ToDoId <- toDoId

        let insertToDo (command : createToDoCommand.command) =
            try
                let context = getContext()
                let todo = context.Public.ToDo.Create()
                todo.Id <- command.id
                todo.Description <- command.description
                todo.CreatorId <- command.creatorId
                
                command.tagsIds |> Seq.iter (linkToDoAndTag context command.id)
                                
                context.SubmitUpdates()
                Success command
            with 
                | ex -> Error(Sentences.Error.databaseFailure, 
                              Infrastructure.Error.getExceptionMessages ex)
            