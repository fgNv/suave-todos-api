using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoMigrations.Models;

namespace TodoMigrations.Models
{
    [Table("ToDo", Schema = "public")]
    public class ToDo
    {
        public ToDo()
        {
            Tags = new Collection<Tag>();
        }

        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public User Creator { get; set; }
        
        public ICollection<Tag> Tags { get; set; }
    }
}
