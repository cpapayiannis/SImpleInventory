using SimpleInventory.DataAccess.DAOs;

namespace SimpleInventory.DataAccess.Classes
{
    public class ProductListResponse
    {
        public required IEnumerable<ProductDAO> Items { get; init; }
        public required int Total { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
    }
}