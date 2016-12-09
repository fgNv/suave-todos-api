module PgSqlPersistence
    open FSharp.Data.Sql
    open System.IO

    [<Literal>]
    let connectionString = @"User ID=homestead;Password=secret;
                             Host=192.168.36.36;Port=5432;Database=ingresso_2;"

    [<Literal>]
    let resolutionPath = "..\\..\\..\\packages\\Npgsql.3.1.9\\lib\\net451\\Npgsql.dll"

    type private pgsqlAccess = SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL,
                                               connectionString,
                                               ResolutionPath = resolutionPath>

    module User =
        let userExists id =
            let context = pgsqlAccess.GetDataContext()
            context.Public.User |> Seq.exists(fun u -> u.Id = id)
    
    module ToDo =
        open Business
        open Infrastructure.Railroad

        let private linkToDoAndTag (context : pgsqlAccess.dataContext) toDoId tagId =
            let toDoTagLink = context.Public.ToDoTags.Create()
            toDoTagLink.TagId <- tagId
            toDoTagLink.ToDoId <- toDoId

        let insertToDo (command : createToDoCommand.command) =
            try
                let context = pgsqlAccess.GetDataContext()
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
            