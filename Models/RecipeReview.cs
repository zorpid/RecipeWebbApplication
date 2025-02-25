using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class RecipeReview
    {
        [Key]
        public int Id { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }  // 1-5 star rating

        [StringLength(1000)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
