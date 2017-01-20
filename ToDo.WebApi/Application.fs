module Application    

open Business

module Persistence = PgSqlPersistence

module User =
    let createUser =
        Business.CreateUser.handle Persistence.User.insertUser

module Tag =
    let createTag =
        Business.CreateTag.handle
            Persistence.User.userExists Persistence.Tag.insertTag

module ToDo =
    let createToDo =
        Business.CreateToDo.handle 
            Persistence.User.userExists Persistence.ToDo.insertToDo
