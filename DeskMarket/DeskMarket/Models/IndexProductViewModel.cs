﻿namespace DeskMarket.Models
{
    public class IndexProductViewModel
    {
        public string? ImageUrl { get; set; }

        public string ProductName { get; set; } = null!;

        public decimal Price { get; set; }

        public bool IsSeller { get; set; }

        public bool HasBought { get; set; }

        public int Id { get; set; }

    }
}
