namespace ToDoMigrations.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class sat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.ToDo",
                c => new
                    {
                        id = c.Guid(nullable: false),
                        Description = c.String(nullable: false, maxLength: 100),
                        CreatedAt = c.DateTime(nullable: false),
                        User_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("public.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "public.User",
                c => new
                    {
                        id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.ToDo", "User_Id", "public.User");
            DropIndex("public.ToDo", new[] { "User_Id" });
            DropTable("public.User");
            DropTable("public.ToDo");
        }
    }
}
