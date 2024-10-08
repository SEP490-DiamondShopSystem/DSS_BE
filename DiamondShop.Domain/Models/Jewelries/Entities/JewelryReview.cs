﻿using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;

namespace DiamondShop.Domain.Models.Jewelries.Entities
{
    public class JewelryReview : Entity<JewelryId>
    {
        public AccountId AccountId { get; set; }
        public Account Account { get; set; }
        public string Content { get; set; }
        public int StarRating { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsHidden { get; set; }
        public List<Media>? Images { get; set; } = new();
        public JewelryReview() { }
    }
}
