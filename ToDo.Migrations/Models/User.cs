using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToDoMigrations.Models
{
    [Table("User", Schema = "public")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Name { get; set; }

        public ICollection<TodoMigrations.Models.ToDo> ToDos { get; set; }
    }
}
