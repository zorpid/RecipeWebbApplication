namespace RecipeWebbApplication.Models
{
    // Junction Table: Recipe ↔ Ingredients
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }

        public string Quantity { get; set; }  // E.g., "2 cups", "1 tbsp"
    }
}
