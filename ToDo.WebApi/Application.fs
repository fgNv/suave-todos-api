module Application    

open Business

module Persistence = PgSqlPersistence

module User =
    let createUser =
        Business.createUserCommand.handle Persistence.User.insertUser

module Tag =
    let createTag =
        Business.createTagCommand.handle
            Persistence.User.userExists Persistence.Tag.insertTag

module ToDo =
    let createToDo =
        Business.createToDoCommand.handle 
            Persistence.User.userExists Persistence.ToDo.insertToDo
