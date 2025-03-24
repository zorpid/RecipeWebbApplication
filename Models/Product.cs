using System.ComponentModel.DataAnnotations;

namespace RecipeWebbApplication.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }
}
