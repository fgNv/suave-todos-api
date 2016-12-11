namespace ToDoMigrations.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TagCreator : DbMigration
    {
        public override void Up()
        {
            AddColumn("public.Tag", "Creator_Id", c => c.Guid(nullable: false));
            CreateIndex("public.Tag", "Creator_Id");
            AddForeignKey("public.Tag", "Creator_Id", "public.User", "id");
        }
        
        public override void Down()
        {
            DropForeignKey("public.Tag", "Creator_Id", "public.User");
            DropIndex("public.Tag", new[] { "Creator_Id" });
            DropColumn("public.Tag", "Creator_Id");
        }
    }
}
