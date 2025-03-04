using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Data;
using RecipeWebbApplication.Models;

namespace RecipeWebbApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context; // Inject the database context
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
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
    }
}
