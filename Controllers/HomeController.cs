using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Data;
using RecipeWebbApplication.Models;

namespace RecipeWebbApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Inject the database context
        public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.FeaturedRecipes = await _context.Recipes
                .OrderByDescending(r => r.CreatedAt)
                .Take(6)
                .ToListAsync(); // Get latest 6 recipes

            ViewBag.Categories = await _context.Categories.ToListAsync(); // Get all categories

            ViewBag.PopularTags = await _context.Tags
                .OrderByDescending(t => t.RecipeTags.Count)
                .Take(5)
                .ToListAsync(); // Get top 5 popular tags

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Action method to show the profile page
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            //var userId = _userManager.GetUserId(User);
            //if (userId == null)
            //{
            //    return RedirectToAction("Login", "Account");
            //}

            //var recipes = await _context.Recipes
            //    .Where(r => r.CreatedByUserId == userId)
            //    .Include(r => r.CreatedByUser)
            //    .ToListAsync();

            //return View(recipes);


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userRecipes = _context.Recipes
                .Where(r => r.CreatedByUserId == userId)
                .Include(r => r.CreatedByUser)// Show only logged-in user's recipes
                .ToList();


            return View(userRecipes);




        }
        public async Task<IActionResult> CartIcon()
        {
            int cartCount = 0;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = _userManager.GetUserId(User);
                cartCount = await _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .SumAsync(ci => ci.Quantity);
            }

            return PartialView("_CartIcon", cartCount);
        }


    }
}
