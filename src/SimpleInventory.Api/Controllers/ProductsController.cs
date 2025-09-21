using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleInventory.Common.Classes;
using SimpleInventory.DataAccess.DTOs;
using SimpleInventory.DataAccess.Intefaces;

namespace SimpleInventory.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ISimpleInventoryRepository _repo;

        public ProductsController(ISimpleInventoryRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] string? q, [FromQuery] int? categoryId,
                                                 [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var opts = new ProductQueryOptions { Search = q, CategoryId = categoryId, Page = page, PageSize = pageSize };
            var result = await _repo.GetProductsAsync(opts);

            return Ok(new
            {
                items = result.Items.Select(p => new ProductDTO
                {
                    Sku = p.Sku,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryId = p.CategoryId,
                    UpdatedAt = p.UpdateAt
                }),
                total = result.TotalItems,
                page = result.Page,
                pageSize = result.PageSize
            });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _repo.GetProductByIdAsync(id);
            return product is null ? NotFound() : Ok(new ProductDTO
            {
                Sku = product.Sku,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                UpdatedAt = product.UpdateAt
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (await _repo.ProductSkuExistsAsync(dto.Sku))
            {
                ModelState.AddModelError(nameof(ProductDTO.Sku), "SKU must be unique.");
                return ValidationProblem(ModelState);
            }

            var created = await _repo.CreateProductAsync(new Product
            {
                Sku = dto.Sku,
                Name = dto.Name,
                Price = dto.Price,
                Quantity = dto.Quantity,
                CategoryId = dto.CategoryId
            });

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, ProductDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (await _repo.ProductSkuExistsAsync(dto.Sku, excludingId: id))
            {
                ModelState.AddModelError(nameof(ProductDTO.Sku), "SKU must be unique.");
                return ValidationProblem(ModelState);
            }

            var updated = await _repo.UpdateProductAsync(new Product
            {
                Id = id,
                Sku = dto.Sku,
                Name = dto.Name,
                Price = dto.Price,
                Quantity = dto.Quantity,
                CategoryId = dto.CategoryId
            });

            return updated is null ? NotFound() : Ok(dto);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteProductAsync(id);
            return deleted is null ? NotFound() : NoContent();
        }
    }
}
