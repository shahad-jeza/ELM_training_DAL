using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }

        [Required]
        [StringLength(100)]
        public string Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}