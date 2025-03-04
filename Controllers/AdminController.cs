using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Data;  // Make sure this namespace is correct
using RecipeWebbApplication.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeWebbApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context; // Add database context
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context; // Assign database context
            _userManager = userManager;
            _roleManager = roleManager;
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




    }
}
