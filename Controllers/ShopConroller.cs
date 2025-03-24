using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeWebbApplication.Data;
using RecipeWebbApplication.Data;           // namespace for ApplicationDbContext
using RecipeWebbApplication.Models;         // namespace for ShopItem and CartItem

public class ShopController : Controller
{
    private readonly ApplicationDbContext _db;
    public ShopController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Shop
    public async Task<IActionResult> Index()
    {
        // Retrieve all products from the database
        var products = await _db.ShopItems.ToListAsync();
        return View(products);
    }
}
