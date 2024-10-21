using Microsoft.AspNetCore.Identity;

namespace DeskMarket.Models
{
    public class DeleteProductViewModel
    {
        public string ProductName { get; set; } = null!;

        public IdentityUser Seller { get; set; } = null!;

        public int Id { get; set; }

        public string SellerId { get; set; } = null!;
    }
}
