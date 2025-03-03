using Microsoft.AspNetCore.Identity;

namespace RecipeWebbApplication.Models
{
    public class ApplicationUser : IdentityUser
    {
        // You can add custom fields here
        public string FullName { get; set; } = string.Empty; // Ensure it has a default value

        // Navigation properties
        public List<Recipe> Recipes { get; set; }
        public List<RecipeReview> Reviews { get; set; }
    }
}
