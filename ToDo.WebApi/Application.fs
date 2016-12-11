module Application    
    open Business

    module Persistence = PgSqlPersistence

    module Tag =
        let createTag =
            Business.createTagCommand.handle
                Persistence.User.userExists Persistence.Tag.createTag

    module ToDo =
        

        let createToDo =
            Business.createToDoCommand.handle 
                Persistence.User.userExists Persistence.ToDo.insertToDo

