
// Controllers/CartController.cs
using Microsoft.AspNetCore.Mvc;
using RecipeWebbApplicationMVC.Services;

namespace RecipeWebbApplicationMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ShoppingCartService _shoppingCartService;

        public CartController(ShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            return View(_shoppingCartService.GetCartItems());
        }

        public IActionResult AddToCart(int id)
        {
            // Add logic to add item to cart
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            // Add logic to remove item from cart
            return RedirectToAction("Index");
        }
    }
}

// Controllers/CheckoutController.cs
using Microsoft.AspNetCore.Mvc;
using RecipeWebbApplicationMVC.Services;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;

namespace RecipeWebbApplicationMVC.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ShoppingCartService _shoppingCartService;

        public CheckoutController(ShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateSession()
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = _shoppingCartService.GetCartItems().Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Name,
                        },
                    },
                    Quantity = item.Quantity,
                }).ToList(),
                Mode = "payment",
                SuccessUrl = "https://yourdomain.com/success",
                CancelUrl = "https://yourdomain.com/cancel",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
    }
}
