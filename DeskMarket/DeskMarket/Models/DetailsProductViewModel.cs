﻿namespace DeskMarket.Models
{
    public class DetailsProductViewModel
    {
        public string? ImageUrl { get; set; }

        public string ProductName { get; set; } = null!;

        public decimal Price { get; set; }

        public string Description { get; set; } = null!;

        public string CategoryName { get; set; } = null!;

        public string AddedOn { get; set; } = null!;

        public string Seller { get; set; } = null!;

        public bool HasBought { get; set; }

        public int Id { get; set; }

    }
}