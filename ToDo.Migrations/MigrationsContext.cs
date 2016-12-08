using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoMigrations.Models;

namespace TodoMigrations
{
    [DbConfigurationType(typeof(NpgsqlConfiguration))]
    class MigrationsContext : DbContext
    {
        public DbSet<Models.ToDo> ToDos { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<MigrationsContext>(null);
            modelBuilder.HasDefaultSchema("public");
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }
        
        public MigrationsContext(): base("User ID=homestead;Password=secret;Host=192.168.36.36;Port=5432;Database=ingresso_2;Pooling=true;")
        {

        }
    }
}
