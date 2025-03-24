using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class ShopItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        //[StringLength(500)]
        //public string Description { get; set; }
        //public string ImageUrl { get; set; }
    }
}
