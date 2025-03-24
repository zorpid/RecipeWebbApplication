namespace RecipeWebbApplication.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;    // ID of the AspNetUser (from Identity)
        public int ShopItemId { get; set; }                   // Foreign key to ShopItem
        public int Quantity { get; set; }


        public ShopItem? ShopItem { get; set; }
    }
}
