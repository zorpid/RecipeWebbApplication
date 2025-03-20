using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class ApplicationUser : IdentityUser
    {
        // You can add custom fields here
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty; // Ensure it has a default value

        // Navigation properties
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>(); // Initialize with default value
        public List<RecipeReview> Reviews { get; set; } = new List<RecipeReview>(); // Initialize with default value
    }
}