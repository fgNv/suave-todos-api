namespace ToDoMigrations.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class model100 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "public.Tag",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "public.ToDo",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Description = c.String(nullable: false, maxLength: 100),
                        CreatedAt = c.DateTime(nullable: false),
                        Creator_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("public.User", t => t.Creator_Id)
                .Index(t => t.Creator_Id);
            
            CreateTable(
                "public.User",
                c => new
                    {
                        id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "public.ToDoTags",
                c => new
                    {
                        ToDo_Id = c.Guid(nullable: false),
                        Tag_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.ToDo_Id, t.Tag_Id })
                .ForeignKey("public.ToDo", t => t.ToDo_Id, cascadeDelete: true)
                .ForeignKey("public.Tag", t => t.Tag_Id, cascadeDelete: true)
                .Index(t => t.ToDo_Id)
                .Index(t => t.Tag_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("public.ToDoTags", "Tag_Id", "public.Tag");
            DropForeignKey("public.ToDoTags", "ToDo_Id", "public.ToDo");
            DropForeignKey("public.ToDo", "Creator_Id", "public.User");
            DropIndex("public.ToDoTags", new[] { "Tag_Id" });
            DropIndex("public.ToDoTags", new[] { "ToDo_Id" });
            DropIndex("public.ToDo", new[] { "Creator_Id" });
            DropTable("public.ToDoTags");
            DropTable("public.User");
            DropTable("public.ToDo");
            DropTable("public.Tag");
        }
    }
}
