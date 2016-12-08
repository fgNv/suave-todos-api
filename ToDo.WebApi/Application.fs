module Application

    module ToDo =
        open Business

        let createToDo =
            Business.createToDoCommand.handle PgSqlPersistence.ToDo.saveToDo

