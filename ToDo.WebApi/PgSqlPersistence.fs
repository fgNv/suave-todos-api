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

    module ToDo =
        open Business
        open Infrastructure.Railroad

        let saveToDo (command : createToDoCommand.command) =
            try
                let context = pgsqlAccess.GetDataContext()            
                let todo = context.Public.ToDo.Create()
                todo.Id <- command.id
                todo.Description <- command.description
                context.SubmitUpdates()
                Success command
            with 
                | ex -> Error(Sentences.Error.databaseFailure, 
                              Infrastructure.Error.getExceptionMessages ex)
            