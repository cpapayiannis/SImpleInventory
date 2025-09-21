using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleInventory.DataAccess.DAOs
{
    [Table("Product")]
    public class ProductDAO
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(32, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 32 characters")]
        [Column("sku")]
        public string Sku { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Price must be >= 0")]
        [Column("price", TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be >= 0")]
        [Column("quantity")]
        public int Quantity { get; set; }

        [ForeignKey(nameof(Category))]
        [Column("category_id")]
        public int CategoryId { get; set; }
        public CategoryDAO Category { get; set; } = null!;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}