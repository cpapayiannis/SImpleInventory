using SimpleInventory.Common.Classes;

namespace SimpleInventory.DataAccess.Intefaces
{
    public interface ISimpleInventoryRepository
    {
        //Category
        Task<List<Category>> GetAllCategoriesAsync();
        Task<bool> CategoryExistAsync(Category category);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(Category category);

        //Product
        Task<PagedResult<Product>> GetProductsAsync(ProductQueryOptions options);
        Task<Product?> GetProductByIdAsync(int id);

        Task<Product> CreateProductAsync(Product product);
        Task<Product?> UpdateProductAsync(Product product);
        Task<Product?> DeleteProductAsync(int id);
        Task<bool> ProductSkuExistsAsync(string sku, int? excludingId = null);
    }
}
