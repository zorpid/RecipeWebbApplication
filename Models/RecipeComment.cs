using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class RecipeComment
    {
        [Key]
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
