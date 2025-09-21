using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleInventory.Common.Classes;
using SimpleInventory.DataAccess.Intefaces;

namespace SimpleInventory.Web.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly ISimpleInventoryRepository _repo;

        public CategoriesController(ISimpleInventoryRepository repo) => _repo = repo;

        public async Task<IActionResult> Index()
        {
            var categories = await _repo.GetAllCategoriesAsync();
            return View(categories); 
        }

        public IActionResult Create() => View(new Category());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _repo.CategoryExistAsync(model))
            {
                ModelState.AddModelError(nameof(Category.Name), "Category already exists.");
                return View(model);
            }

            await _repo.CreateCategoryAsync(model);
            TempData["Success"] = "Category created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var categories = await _repo.GetAllCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == id);

            if (category is null) return NotFound();

            var ok = await _repo.DeleteCategoryAsync(category);
            TempData[ok ? "Success" : "Error"] =
                ok ? "Category deleted." : "Cannot delete category with products.";
            return RedirectToAction(nameof(Index));
        }
    }
}
