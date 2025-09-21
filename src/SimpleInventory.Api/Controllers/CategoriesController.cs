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
    public class CategoriesController : ControllerBase
    {
        private readonly ISimpleInventoryRepository _repo;

        public CategoriesController(ISimpleInventoryRepository repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var all = await _repo.GetAllCategoriesAsync();
            return Ok(all.Select(c => new CategoryDTO { Name = c.Name }));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryDTO dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var candidate = new Category { Name = dto.Name };
            if (await _repo.CategoryExistAsync(candidate))
            {
                ModelState.AddModelError(nameof(CategoryDTO.Name), "Category name must be unique.");
                return ValidationProblem(ModelState);
            }

            var created = await _repo.CreateCategoryAsync(candidate);
            return CreatedAtAction(nameof(GetAll), new { }, new CategoryDTO { Name = created.Name });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var all = await _repo.GetAllCategoriesAsync();
            var cat = all.FirstOrDefault(c => c.Id == id);
            if (cat is null) return NotFound();

            var ok = await _repo.DeleteCategoryAsync(cat);
            if (!ok)
            {
                return Conflict(new ProblemDetails
                {
                    Title = "Category is in use",
                    Detail = "Cannot delete category because products reference it.",
                    Status = StatusCodes.Status409Conflict
                });
            }

            return NoContent();
        }
    }
}
