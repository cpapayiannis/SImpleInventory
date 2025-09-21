using SimpleInventory.Common.Enums;

namespace SimpleInventory.Common.Classes
{
    public sealed class ProductQueryOptions
    {
        public string? Search { get; set; }       // name or sku
        public int? CategoryId { get; set; }    // FK filter (adjust type)
        public ProductSort Sort { get; set; } = ProductSort.NameAsc;
        public int Page { get; set; } = 1;        // 1-based
        public int PageSize { get; set; } = 10;   // default 10
    }
}
