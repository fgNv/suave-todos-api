using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoMigrations.Models
{
    [Table("ToDo", Schema = "public")]
    public class ToDo
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [MaxLength(100)]
        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
