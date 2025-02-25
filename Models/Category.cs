using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public List<Recipe> Recipes { get; set; }
    }

}
