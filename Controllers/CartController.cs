using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Stripe;
using Stripe.Checkout;
using RecipeWebbApplication.Data;
using RecipeWebbApplication.Models;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config; // for accessing Stripe settings (if needed)

    public CartController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // POST: /Cart/Add
    [HttpPost]
    public async Task<IActionResult> Add(int productId, int quantity = 1)
    {
        // Get the current user's ID (using Identity)
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Challenge(); // Forces login if not already logged in
        }

        // Find if this product is already in the user's cart
        var existingItem = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ShopItemId == productId);
        if (existingItem != null)
        {
            // If it exists, update the quantity
            existingItem.Quantity += quantity;
            _db.CartItems.Update(existingItem);
        }
        else
        {
            // If not, add a new CartItem
            var product = await _db.ShopItems.FindAsync(productId);
            if (product == null)
                return NotFound(); // product not found

            var cartItem = new CartItem
            {
                UserId = userId,
                ShopItemId = product.Id,
                Quantity = quantity
            };
            _db.CartItems.Add(cartItem);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");  // Go back to cart view
    }

    // GET: /Cart (view cart items)
    public async Task<IActionResult> Index()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Include product details for each cart item
        var cartItems = await _db.CartItems
                                 .Include(ci => ci.ShopItem)
                                 .Where(ci => ci.UserId == userId)
                                 .ToListAsync();
        return View(cartItems);
    }

    // POST: /Cart/Update (update quantity for an item)
    [HttpPost]
    public async Task<IActionResult> Update(int cartItemId, int quantity)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartItem = await _db.CartItems.FindAsync(cartItemId);
        if (cartItem == null || cartItem.UserId != userId)
        {
            return NotFound();
        }

        if (quantity <= 0)
        {
            // Remove the item if quantity is zero or negative
            _db.CartItems.Remove(cartItem);
        }
        else
        {
            // Update the quantity
            cartItem.Quantity = quantity;
            _db.CartItems.Update(cartItem);
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST: /Cart/Remove (remove an item from cart)
    [HttpPost]
    public async Task<IActionResult> Remove(int cartItemId)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartItem = await _db.CartItems.FindAsync(cartItemId);
        if (cartItem != null && cartItem.UserId == userId)
        {
            _db.CartItems.Remove(cartItem);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    // POST: /Cart/Checkout (initiate Stripe checkout)
    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartItems = await _db.CartItems
                                 .Include(ci => ci.ShopItem)
                                 .Where(ci => ci.UserId == userId)
                                 .ToListAsync();
        if (cartItems.Count == 0)
        {
            // No items in cart, nothing to checkout
            return RedirectToAction("Index");
        }

        // Load secret key from configuration (if not set globally)
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        // Build the Stripe checkout session options with line items from cart
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = new List<string> { "card" },       // accept card payments&#8203;:contentReference[oaicite:0]{index=0}
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",                                       // one-time payment (not subscription)
            SuccessUrl = $"{Request.Scheme}://{Request.Host}/Cart/Success",
            CancelUrl = $"{Request.Scheme}://{Request.Host}/Cart/Cancel"
        };

        foreach (var item in cartItems)
        {
            // Each cart item becomes a Stripe line item
            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ShopItem.Name
                    },
                    UnitAmount = (long)(item.ShopItem.Price * 100)   // price in cents
                },
                Quantity = item.Quantity
            });
        }

        var service = new SessionService();
        Session session = service.Create(options);                 // Create Stripe session&#8203;:contentReference[oaicite:1]{index=1}

        // Redirect the user to the Stripe Checkout page
        return Redirect(session.Url);
    }

    // GET: /Cart/Success (Stripe checkout success redirect)
    public async Task<IActionResult> Success()
    {
        // (Optional) After successful payment, you might clear the user's cart or create an Order record.
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        // Example: clear cart
        var userCartItems = _db.CartItems.Where(ci => ci.UserId == userId);
        _db.CartItems.RemoveRange(userCartItems);
        await _db.SaveChangesAsync();

        return View();
    }

    // GET: /Cart/Cancel (Stripe checkout canceled redirect)
    public IActionResult Cancel()
    {
        // Just show a cancellation message or redirect back to cart
        return View();
    }
}
