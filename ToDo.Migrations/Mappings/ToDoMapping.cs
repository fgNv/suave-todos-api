using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoMigrations.Models;

namespace ToDoMigrations.Mappings
{
    class ToDoMapping : EntityTypeConfiguration<ToDo>
    {
        public ToDoMapping()
        {
            HasMany(todo => todo.Tags).WithMany();
        }
    }
}
