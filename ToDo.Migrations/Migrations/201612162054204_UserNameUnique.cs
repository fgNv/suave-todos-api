namespace ToDoMigrations.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserNameUnique : DbMigration
    {
        public override void Up()
        {
            CreateIndex("public.User", "Name", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("public.User", new[] { "Name" });
        }
    }
}
