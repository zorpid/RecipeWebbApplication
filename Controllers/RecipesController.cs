using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Models;

namespace RecipeWebbApplication.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RecipesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> RecipeDisplay()
        {
            var recipes = await _context.Recipes
                .Include(r => r.Category) // Include category details
                .ToListAsync();

            return View(recipes); // This will now look for a RecipeDisplay.cshtml view
        }

        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Recipes.Include(r => r.Category).Include(r => r.CreatedByUser);
            return View(await applicationDbContext.ToListAsync());
        }



        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.CreatedByUser)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient) // Include ingredients
                .Include(r => r.RecipeReviews) // Include reviews
                .FirstOrDefaultAsync(m => m.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }


        // GET: Recipes/Create
        public IActionResult Create()
        {
            //  Populate Category dropdown only if categories exist
            ViewData["CategoryId"] = _context.Categories.Any()
                ? new SelectList(_context.Categories, "Id", "Name")
                : null; // Don't set a dropdown if there are no categories

            // Removed CreatedByUserId (should be automatically set in POST)

            //  Populate Difficulty dropdown
            ViewBag.DifficultyList = new SelectList(Enum.GetValues(typeof(DifficultyLevel)));

            return View();
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Instructions,PrepTimeMinutes,CookTimeMinutes,Servings,ImageUrl,Difficulty,CategoryId")] Recipe recipe)
        {
            //if (ModelState.IsValid)
            //{
            // Automatically set timestamps
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.UpdatedAt = DateTime.UtcNow;

            //  Set CreatedByUserId (if users exist, otherwise allow null)
            recipe.CreatedByUserId = null; // Or use _userManager.GetUserId(User) if authentication exists

            //  Handle CategoryId (if it’s nullable)
            if (recipe.CategoryId == null)
            {
                recipe.CategoryId = 1; // Set to a default category (optional)
            }

            _context.Add(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            //}

            //  Re-populate dropdowns if validation fails
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", recipe.CategoryId);
            ViewBag.DifficultyList = new SelectList(Enum.GetValues(typeof(DifficultyLevel)));

            return View(recipe);
        }

        // GET: Recipes/Edit/5
        // GET: Recipes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Category) // Ensure Category is included  
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient) // Include ingredients
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", recipe.CategoryId);
            ViewBag.DifficultyList = new SelectList(Enum.GetValues(typeof(DifficultyLevel)));

            return View(recipe);
        }



        // POST: Recipes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Instructions,PrepTimeMinutes,CookTimeMinutes,Servings,ImageUrl,Difficulty,CategoryId")] Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            //if (ModelState.IsValid)
            //{
            try
            {
                // Attach entity manually
                var existingRecipe = await _context.Recipes.FindAsync(id);
                if (existingRecipe == null)
                {
                    return NotFound();
                }

                // Update only modified fields
                existingRecipe.Name = recipe.Name;
                existingRecipe.Description = recipe.Description;
                existingRecipe.Instructions = recipe.Instructions;
                existingRecipe.PrepTimeMinutes = recipe.PrepTimeMinutes;
                existingRecipe.CookTimeMinutes = recipe.CookTimeMinutes;
                existingRecipe.Servings = recipe.Servings;
                existingRecipe.ImageUrl = recipe.ImageUrl;
                existingRecipe.Difficulty = recipe.Difficulty;

                // Explicitly update CategoryId
                existingRecipe.CategoryId = recipe.CategoryId;
                _context.Entry(existingRecipe).Property(r => r.CategoryId).IsModified = true;

                existingRecipe.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Recipes.Any(e => e.Id == recipe.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(RecipeDisplay)); // Redirect back to list
            //}

            // Log ModelState errors if validation fails
            //foreach (var modelState in ModelState.Values)
            //{
            //    foreach (var error in modelState.Errors)
            //    {
            //        Console.WriteLine($"Validation Error: {error.ErrorMessage}");
            //    }
            //}

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", recipe.CategoryId);
            ViewBag.DifficultyList = new SelectList(Enum.GetValues(typeof(DifficultyLevel)));

            return View(recipe);
        }


        // GET: Recipes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // POST: Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeExists(int id)
        {
            return _context.Recipes.Any(e => e.Id == id);
        }
        [HttpPost]
        public async Task<IActionResult> AddIngredient(int recipeId, string ingredientName, string quantity)
        {
            if (string.IsNullOrEmpty(ingredientName) || string.IsNullOrEmpty(quantity))
            {
                return BadRequest("Ingredient name and quantity are required.");
            }

            // Check if the ingredient already exists
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == ingredientName);

            if (ingredient == null)
            {
                // Create new ingredient if it doesn't exist
                ingredient = new Ingredient { Name = ingredientName };
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();
            }

            // Add the relationship
            var recipeIngredient = new RecipeIngredient
            {
                RecipeId = recipeId,
                IngredientId = ingredient.Id,
                Quantity = quantity
            };

            _context.RecipeIngredients.Add(recipeIngredient);
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", new { id = recipeId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveIngredient(int recipeId, int ingredientId)
        {
            var recipeIngredient = await _context.RecipeIngredients
                .FirstOrDefaultAsync(ri => ri.RecipeId == recipeId && ri.IngredientId == ingredientId);

            if (recipeIngredient != null)
            {
                _context.RecipeIngredients.Remove(recipeIngredient);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Edit", new { id = recipeId });
        }
    }
}