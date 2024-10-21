namespace DeskMarket.Models
{
    public class CartProductViewModel
    {
        public string? ImageUrl { get; set; }

        public string ProductName { get; set; } = null!;

        public decimal Price { get; set; }

        public int Id { get; set; }
    }
}
