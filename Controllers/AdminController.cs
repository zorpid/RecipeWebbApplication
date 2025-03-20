using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Data;  // Make sure this namespace is correct
using RecipeWebbApplication.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
namespace RecipeWebbApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context; // Add database context
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _context = context; // Assign database context
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> UserManagement()
        {
            var users = _userManager.Users.ToList();
            var userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] = (await _userManager.GetRolesAsync(user)).ToList();
            }

            ViewBag.UserRoles = userRoles; // Pass roles to the view
            return View(users);
        }

        public async Task<IActionResult> AssignRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.Roles = roles;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.AddToRoleAsync(user, role);
            return RedirectToAction("UserManagement");
        }

        public async Task<IActionResult> RoleManagement()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            return RedirectToAction("RoleManagement");
        }

        public async Task<IActionResult> RecipeManagement()
        {
            var recipes = _context.Recipes.ToList();
            return View(recipes);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("RecipeManagement");
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, [Bind("Id,FullName,Email")] ApplicationUser user)
        {
            _logger.LogInformation("EditUser POST method called with id: {Id}", id);

            if (id != user.Id)
            {
                _logger.LogWarning("User ID mismatch. Provided ID: {ProvidedId}, User ID: {UserId}", id, user.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Model state is valid for user ID: {UserId}", user.Id);

                var existingUser = await _userManager.FindByIdAsync(id);
                if (existingUser == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", id);
                    return NotFound();
                }

                _logger.LogInformation("User found with ID: {UserId}. Updating user details.", id);

                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;

                // Initialize Recipes and Reviews if they are null
                if (existingUser.Recipes == null)
                {
                    existingUser.Recipes = new List<Recipe>();
                }
                if (existingUser.Reviews == null)
                {
                    existingUser.Reviews = new List<RecipeReview>();
                }

                var result = await _userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User details updated successfully for user ID: {UserId}", user.Id);
                    return RedirectToAction(nameof(UserManagement));
                }

                _logger.LogError("Failed to update user details for user ID: {UserId}. Errors: {Errors}", user.Id, result.Errors);
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid for user ID: {UserId}", user.Id);
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Validation error: {ErrorMessage}", error.ErrorMessage);
                }
            }

            return View(user);
        }





        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(UserManagement));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(user);
        }
    }
}
