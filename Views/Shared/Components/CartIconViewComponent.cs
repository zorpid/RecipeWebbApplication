namespace RecipeWebbApplication.Views.Shared.Components
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using RecipeWebbApplication.Models;
    using RecipeWebbApplication.Data;

    public class CartIconViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartIconViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int cartCount = 0;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = _userManager.GetUserId(UserClaimsPrincipal);
                cartCount = await _context.CartItems
                    .Where(ci => ci.UserId == userId)
                    .SumAsync(ci => ci.Quantity);
            }

            return View(cartCount); // Renders Default.cshtml in the view path
        }
    }

}
