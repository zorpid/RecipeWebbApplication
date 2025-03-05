using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RecipeWebbApplication.Data;
using RecipeWebbApplication.Models;

namespace RecipeWebbApplication.Controllers
{
    public class RecipesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> RecipeDisplay(int? categoryId, int? tagId)
        {

            var recipes = _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.RecipeTags)
                    .ThenInclude(rt => rt.Tag)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                recipes = recipes.Where(r => r.CategoryId == categoryId);
                ViewBag.CategoryName = _context.Categories
                    .Where(c => c.Id == categoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault();
            }

            if (tagId.HasValue)
            {
                recipes = recipes.Where(r => r.RecipeTags.Any(rt => rt.TagId == tagId));
                ViewBag.TagName = _context.Tags
                    .Where(t => t.Id == tagId)
                    .Select(t => t.Name)
                    .FirstOrDefault();
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Tags = await _context.Tags.ToListAsync();

            return View(await recipes.ToListAsync());
        }


        // GET: Recipes
        public async Task<IActionResult> Index()
        {
            //var applicationDbContext = _context.Recipes.Include(r => r.Category).Include(r => r.CreatedByUser);
            //return View(await applicationDbContext.ToListAsync());


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var recipes = await _context.Recipes
                .Include(r => r.Category) // Include category details
                .Include(r => r.CreatedByUser) // Ensure CreatedByUser is included
                .Where(r => isAdmin || r.IsPublic || r.CreatedByUserId == userId) // Show public or user-owned recipes
                .ToListAsync();

            return View(recipes);

        }



        // GET: Recipes/Details/5
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
                .Include(r => r.RecipeTags) // ✅ Include RecipeTags
                    .ThenInclude(rt => rt.Tag) // ✅ Include related Tags
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
            var recipe = new Recipe
            {

                RecipeIngredients = new List<RecipeIngredient>(), // Ensure it's initialized
                RecipeTags = new List<RecipeTag>() // Ensure it's initialized
            };

            // Populate Category dropdown only if categories exist
            ViewData["CategoryId"] = _context.Categories.Any()
                ? new SelectList(_context.Categories, "Id", "Name")
                : null;

            // Populate Difficulty dropdown
            ViewBag.DifficultyList = new SelectList(Enum.GetValues(typeof(DifficultyLevel)));

           
            // ✅ Ensure Tags Are Loaded
            var availableTags = _context.Tags.ToList();
            ViewBag.AvailableTags = availableTags ?? new List<Tag>(); // Prevents null error


            // Ensure a temporary ingredient list exists
            ViewBag.TempIngredients = new List<RecipeIngredient>();

            return View(recipe);
        }

        // POST: Recipes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Recipe recipe, List<string> IngredientNames, List<string> IngredientQuantities)
{
    try
    {
        // ✅ Validate IngredientNames list
        if (IngredientNames == null || IngredientNames.Count == 0)
        {
            Console.WriteLine("IngredientNames is null or empty.");
            return BadRequest("Ingredients list cannot be empty.");
        }

        // ✅ Validate that IngredientNames and IngredientQuantities match
        if (IngredientQuantities == null || IngredientNames.Count != IngredientQuantities.Count)
        {
            Console.WriteLine("IngredientNames and IngredientQuantities count mismatch.");
            return BadRequest("Ingredient names and quantities do not match.");
        }

        recipe.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the user ID from the claims
        recipe.CreatedAt = DateTime.UtcNow;
        recipe.UpdatedAt = DateTime.UtcNow;
        recipe.RecipeIngredients = new List<RecipeIngredient>();

        for (int i = 0; i < IngredientNames.Count; i++)
        {
            string ingredientName = IngredientNames[i]?.Trim(); // ✅ Trim to remove spaces
            string ingredientQuantity = IngredientQuantities[i]?.Trim();

            if (string.IsNullOrEmpty(ingredientName) || string.IsNullOrEmpty(ingredientQuantity))
            {
                Console.WriteLine($"Skipping empty ingredient at index {i}");
                continue; // Skip invalid values
            }

            Console.WriteLine($"Processing ingredient: {ingredientName} (Quantity: {ingredientQuantity})");

            // ✅ Fetch ingredient from DB or create new one
            var ingredient = await _context.Ingredients
                .FirstOrDefaultAsync(i => i.Name == ingredientName);

            if (ingredient == null)
            {
                Console.WriteLine($"Ingredient '{ingredientName}' not found. Creating a new one.");
                ingredient = new Ingredient { Name = ingredientName };
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync(); // ✅ Save to get an ID
            }

            // ✅ Attach ingredient to recipe
            recipe.RecipeIngredients.Add(new RecipeIngredient
            {
                RecipeId = recipe.Id,  // ✅ Ensure recipe ID is set
                IngredientId = ingredient.Id,
                Quantity = ingredientQuantity
            });
        }

        _context.Add(recipe);
        await _context.SaveChangesAsync();
        Console.WriteLine("Recipe and ingredients saved successfully!");

        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception in Create method: {ex.Message}");
        return StatusCode(500, "An internal error occurred.");
    }
}



        //// GET: Recipes/Edit/5
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
                .Include(r => r.RecipeTags) // Include Recipe Tags
                    .ThenInclude(rt => rt.Tag) // Include related Tags
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Populate ViewData for dropdowns
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", recipe.CategoryId);
            ViewBag.DifficultyList = new SelectList(Enum.GetValues(typeof(DifficultyLevel)));

            // Load available tags and pre-select current ones
            ViewBag.AvailableTags = _context.Tags.ToList();

            return View(recipe);
        }

        // POST: Recipes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Instructions,PrepTimeMinutes,CookTimeMinutes,Servings,ImageUrl,Difficulty,CategoryId,SelectedTagIds, IsPublic")] Recipe recipe)
        {
            if (id != recipe.Id)
            {
                return NotFound();
            }

            try
            {
                // Attach entity manually
                var existingRecipe = await _context.Recipes
                    .Include(r => r.RecipeTags) // Include existing RecipeTags
                    .FirstOrDefaultAsync(r => r.Id == id);

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
                existingRecipe.CategoryId = recipe.CategoryId;
                existingRecipe.UpdatedAt = DateTime.UtcNow;
                existingRecipe.IsPublic = recipe.IsPublic;

                _context.Entry(existingRecipe).Property(r => r.CategoryId).IsModified = true;

                // Handle updating tags
                if (recipe.SelectedTagIds != null)
                {
                    // Remove old tags
                    existingRecipe.RecipeTags.Clear();

                    // Add new selected tags
                    foreach (var tagId in recipe.SelectedTagIds)
                    {
                        existingRecipe.RecipeTags.Add(new RecipeTag
                        {
                            RecipeId = recipe.Id,
                            TagId = tagId
                        });
                    }
                }

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
        public async Task<IActionResult> AddIngredient(int? recipeId, string ingredientName, string quantity)
        {
            if (string.IsNullOrEmpty(ingredientName) || string.IsNullOrEmpty(quantity))
            {
                return BadRequest("Ingredient name and quantity are required.");
            }

            // Check if the ingredient already exists in the database
            var ingredient = await _context.Ingredients.FirstOrDefaultAsync(i => i.Name == ingredientName);

            if (ingredient == null)
            {
                // If ingredient does not exist, create a new one
                ingredient = new Ingredient { Name = ingredientName };
                _context.Ingredients.Add(ingredient);
                await _context.SaveChangesAsync();
            }

            // Check if we're adding ingredients to an existing recipe (Edit Mode)
            if (recipeId != null && recipeId > 0)
            {
                var existingRecipe = await _context.Recipes
                    .Include(r => r.RecipeIngredients)
                    .FirstOrDefaultAsync(r => r.Id == recipeId);

                if (existingRecipe == null)
                {
                    return NotFound();
                }

                // Add ingredient to the recipe
                existingRecipe.RecipeIngredients.Add(new RecipeIngredient
                {
                    RecipeId = existingRecipe.Id,
                    IngredientId = ingredient.Id,
                    Quantity = quantity
                });

                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = recipeId }); // Redirect back to Edit
            }

            // If we're in Create mode, temporarily store the ingredients (Session or ViewBag)
            var tempIngredients = ViewBag.TempIngredients as List<RecipeIngredient> ?? new List<RecipeIngredient>();

            tempIngredients.Add(new RecipeIngredient
            {
                Ingredient = ingredient, // Associate with newly created ingredient
                Quantity = quantity
            });

            ViewBag.TempIngredients = tempIngredients; // Store temporarily
            return RedirectToAction("Create"); // Redirect back to Create
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

        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var recipes = await _context.Recipes
                .Where(r => r.CreatedByUserId == userId)
                .Include(r => r.CreatedByUser) // Include the creator's user information
                .ToListAsync();

            return View(recipes);
        }



    }
}