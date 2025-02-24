using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        public List<RecipeIngredient> RecipeIngredients { get; set; }
    }
}
