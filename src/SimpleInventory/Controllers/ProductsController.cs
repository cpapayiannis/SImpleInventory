using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleInventory.Common.Classes;
using SimpleInventory.Common.Enums;
using SimpleInventory.DataAccess.Intefaces;

namespace SimpleInventory.Web.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ISimpleInventoryRepository _repo;
        private const int PageSize = 10;

        public ProductsController(ISimpleInventoryRepository repo) => _repo = repo;

        public async Task<IActionResult> Index(string? search, int? categoryId, string? sort, int page = 1)
        {
            var options = new ProductQueryOptions
            {
                Search = search,
                CategoryId = categoryId,
                Page = page,
                PageSize = PageSize,
                Sort = sort switch
                {
                    "name_desc" => ProductSort.NameDesc,
                    "price_asc" => ProductSort.PriceAsc,
                    "price_desc" => ProductSort.PriceDesc,
                    _ => ProductSort.NameAsc
                }
            };

            var result = await _repo.GetProductsAsync(options);

            var categories = await _repo.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            return View(result);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateCategories();
            return View(new Product());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategories(model.CategoryId);
                return View(model);
            }

            if (await _repo.ProductSkuExistsAsync(model.Sku))
            {
                ModelState.AddModelError(nameof(Product.Sku), "SKU must be unique.");
                await PopulateCategories(model.CategoryId);
                return View(model);
            }

            await _repo.CreateProductAsync(model);
            TempData["Success"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _repo.GetProductByIdAsync(id);
            if (product is null) return NotFound();

            await PopulateCategories(product.CategoryId);
            return View(product);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateCategories(model.CategoryId);
                return View(model);
            }

            if (await _repo.ProductSkuExistsAsync(model.Sku, excludingId: id))
            {
                ModelState.AddModelError(nameof(Product.Sku), "SKU must be unique.");
                await PopulateCategories(model.CategoryId);
                return View(model);
            }

            var updated = await _repo.UpdateProductAsync(model);
            if (updated is null) return NotFound();

            TempData["Success"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteProductAsync(id);
            TempData[deleted is null ? "Error" : "Success"] =
                deleted is null ? "Product not found." : "Product deleted.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateCategories(object? selected = null)
        {
            var categories = await _repo.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", selected);
        }
    }
}
