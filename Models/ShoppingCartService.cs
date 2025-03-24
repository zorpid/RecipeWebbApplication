// Services/ShoppingCartService.cs
using System.Collections.Generic;
using System.Linq;

namespace RecipeWebbApplicationMVC.Services
{
    public class ShoppingCartService
    {
        private readonly List<CartItem> _cartItems = new List<CartItem>();

        public void AddToCart(CartItem item)
        {
            _cartItems.Add(item);
        }

        public void RemoveFromCart(CartItem item)
        {
            _cartItems.Remove(item);
        }

        public IEnumerable<CartItem> GetCartItems()
        {
            return _cartItems;
        }

        public decimal GetTotal()
        {
            return _cartItems.Sum(item => item.Price * item.Quantity);
        }
    }
}
