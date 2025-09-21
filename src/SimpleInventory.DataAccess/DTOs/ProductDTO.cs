namespace SimpleInventory.DataAccess.DTOs
{
    public class ProductDTO
    {
        public string Sku { get; set; } = "";
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int CategoryId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
