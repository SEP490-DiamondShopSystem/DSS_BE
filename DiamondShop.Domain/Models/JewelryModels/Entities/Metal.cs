﻿using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiamondShop.Domain.Models.JewelryModels.Entities
{
    public class Metal : Entity<MetalId>
    {
        public string Name { get; set; }
        public string LocalizedName { get; set; }
        public decimal Price { get; set; }
        public Media? Thumbnail { get; set; }
        public Metal() { }
        public static Metal Create(string name, string localizedName, decimal price, MetalId? givenId = null) => new Metal() { Id = givenId is null ? MetalId.Create() : givenId, Name = name, LocalizedName = localizedName, Price = price };
        public void ChangePrice(decimal price)
        {
            if(price < 0)
                throw new Exception("Price cannot be negative");
            var roundedPrice = MoneyVndRoundUpRules.RoundAmountFromDecimal(price);
            Price = roundedPrice;
        }
        [NotMapped]
        public string CodeName
        {
            get
            {
                if (Name.ToLower().Contains("gold"))
                {
                    var words = Name.Split(' ');
                    var name = "";
                    for(int i = 0; i < words.Length; i++)
                    {
                        if (i == 0)
                            name += words[i];
                        else
                            name += words[i].ToUpper()[0];
                    }
                    return name;
                }
                return Name.ToUpper()[0].ToString();
            }
        }
    }
}
