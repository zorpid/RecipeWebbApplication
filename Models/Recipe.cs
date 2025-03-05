using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class Recipe
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Instructions { get; set; }

        public int PrepTimeMinutes { get; set; }

        public int CookTimeMinutes { get; set; }

        public int Servings { get; set; }

        public string ImageUrl { get; set; }

        // Difficulty level (e.g., Easy, Medium, Hard)
        public DifficultyLevel Difficulty { get; set; }

        // Category (e.g., Breakfast, Lunch, Dinner, Dessert)
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        // Foreign Key to User (Creator of the recipe)
        public string? CreatedByUserId { get; set; }
        public ApplicationUser CreatedByUser { get; set; }

        // Many-to-Many Relationship: Recipes ↔ Ingredients
        public List<RecipeIngredient>? RecipeIngredients { get; set; }

        // Many-to-Many Relationship: Recipes ↔ Tags (e.g., Vegan, Keto)
        public List<RecipeTag>? RecipeTags { get; set; }

        // Ratings & Reviews
        public List<RecipeReview>? RecipeReviews { get; set; }

        // Date Created & Updated
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Add this property to handle the selected tags
        public List<int>? SelectedTagIds { get; set; }

        public bool IsPublic { get; set; } // NEW: Determines if the recipe is public or private


    }

    // Difficulty Enum
    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }
}
