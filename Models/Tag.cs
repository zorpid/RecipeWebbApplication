using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public List<RecipeTag>? RecipeTags { get; set; }
    }

}
