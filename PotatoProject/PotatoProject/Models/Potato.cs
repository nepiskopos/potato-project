using System.ComponentModel.DataAnnotations;

namespace PotatoProject.Models
{
    public class Potato
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public required string Description { get; set; }
    }
}
