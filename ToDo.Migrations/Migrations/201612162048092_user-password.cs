namespace ToDoMigrations.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userpassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.User", "Password", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("public.User", "Password");
        }
    }
}
