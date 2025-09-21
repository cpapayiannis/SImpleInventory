using Microsoft.EntityFrameworkCore;
using SimpleInventory.Common.Classes;
using SimpleInventory.Common.Enums;
using SimpleInventory.DataAccess.DAOs;
using SimpleInventory.DataAccess.Intefaces;

namespace SimpleInventory.DataAccess.Classes
{
    public class SimpleInventoryRepository : ISimpleInventoryRepository
    {
        private SimpleInventoryContext _context;
        public SimpleInventoryRepository(SimpleInventoryContext context)
        {
        _context = context;
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories =await _context.Categories.OrderBy(c => c.Name).Select(d => d.DaoToDomain()).ToListAsync();
            return categories;
        } 
        public async Task<bool> CategoryExistAsync(Category category)
        {
            var exists = await _context.Categories.AnyAsync(c => c.Name == category.Name);
            if (exists)
            {
               return true;
            }
            return false;
        }
        public async Task<Category> CreateCategoryAsync(Category category) 
        {
            var categoryDao = category.DomainToDao();
            await _context.Categories.AddAsync(categoryDao);
            await _context.SaveChangesAsync();
            return categoryDao.DaoToDomain();
        }

        public async Task<bool> DeleteCategoryAsync(Category category)
        {
            var categoryDao = await _context.Categories.FindAsync(category.Id);
            if (categoryDao == null)
                return false;

            var inUse = await _context.Set<ProductDAO>()
                                      .AnyAsync(p => p.CategoryId == categoryDao.Id);
            if (inUse)
                return false;

            _context.Categories.Remove(categoryDao);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<PagedResult<Product>> GetProductsAsync(ProductQueryOptions options)
        {
            var q = _context.Set<ProductDAO>()
                       .Include(p => p.Category)
                       .AsQueryable();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                var s = options.Search.Trim();
                q = q.Where(p => p.Name.Contains(s) || p.Sku.Contains(s));
            }

            if (options.CategoryId.HasValue)
                q = q.Where(p => p.CategoryId == options.CategoryId.Value);

            q = options.Sort switch
            {
                ProductSort.NameDesc => q.OrderByDescending(p => p.Name).ThenBy(p => p.Id),
                ProductSort.PriceAsc => q.OrderBy(p => p.Price).ThenBy(p => p.Name),
                ProductSort.PriceDesc => q.OrderByDescending(p => p.Price).ThenBy(p => p.Name),
                _ => q.OrderBy(p => p.Name).ThenBy(p => p.Id),
            };

            var page = Math.Max(1, options.Page);
            var pageSize = Math.Max(1, options.PageSize);

            var total = await q.CountAsync();
            var itemsDao = await q.Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();

            var items = itemsDao.Select(d => d.DaoToDomain()).ToList();

            return new PagedResult<Product>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var dao = await _context.Set<ProductDAO>()
                               .Include(p => p.Category)
                               .FirstOrDefaultAsync(p => p.Id == id);
            return dao?.DaoToDomain();
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Sku) || product.Sku.Length is < 3 or > 32)
                throw new ArgumentException("SKU must be between 3 and 32 characters.", nameof(product.Sku));

            if (string.IsNullOrWhiteSpace(product.Name))
                throw new ArgumentException("Name is required.", nameof(product.Name));

            if (product.Price < 0) throw new ArgumentException("Price must be >= 0.", nameof(product.Price));
            if (product.Quantity < 0) throw new ArgumentException("Quantity must be >= 0.", nameof(product.Quantity));

            if (await ProductSkuExistsAsync(product.Sku))
                throw new InvalidOperationException("SKU must be unique.");

            var dao = product.DomainToDao();
            dao.UpdatedAt = DateTime.UtcNow;

            _context.Set<ProductDAO>().Add(dao);
            await _context.SaveChangesAsync();

            return dao.DaoToDomain();
        }

        public async Task<Product?> UpdateProductAsync(Product product)
        {
            var existing = await _context.Set<ProductDAO>().FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existing == null) return null;

            if (await ProductSkuExistsAsync(product.Sku, product.Id))
                throw new InvalidOperationException("SKU must be unique.");

            existing.Sku = product.Sku;
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Quantity = product.Quantity;
            existing.CategoryId = (int)product.CategoryId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing.DaoToDomain();
        }

        public async Task<Product?> DeleteProductAsync(int id)
        {
            var existing = await _context.Set<ProductDAO>().FirstOrDefaultAsync(p => p.Id == id);
            if (existing == null) return null;

            _context.Set<ProductDAO>().Remove(existing);
            await _context.SaveChangesAsync();

            return existing.DaoToDomain();
        }

        public async Task<bool> ProductSkuExistsAsync(string sku, int? excludingId = null)
        {
            var q = _context.Set<ProductDAO>().AsQueryable().Where(p => p.Sku == sku);
            if (excludingId.HasValue)
                q = q.Where(p => p.Id != excludingId.Value);

            return await q.AnyAsync();
        }

    }
}
