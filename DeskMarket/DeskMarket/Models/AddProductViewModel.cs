using DeskMarket.Data.Models;
using System.ComponentModel.DataAnnotations;
using static DeskMarket.ComanConsts;

namespace DeskMarket.Models
{
    public class AddProductViewModel
    {
        [Required]
        [MaxLength(ProductNameMaxLength)]
        [MinLength(ProductNameMinLength)]
        public string ProductName { get; set; } = null!;

        [Required]
        [Range(ProductPriceMin, ProductPriceMax)]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(ProductDescriptionMaxLength)]
        [MinLength(ProductDescriptionMinLength)]
        public string Description { get; set; } = null!;

        public string? ImageUrl { get; set; }

        [Required]
        public string AddedOn { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
