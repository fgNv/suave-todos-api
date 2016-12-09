module Application    
    module Persistence = PgSqlPersistence

    module ToDo =
        open Business

        let createToDo =
            Business.createToDoCommand.handle 
                Persistence.User.userExists Persistence.ToDo.insertToDo

