﻿using DiamondShop.Domain.Models.AccountAggregate.Entities;
using DiamondShop.Domain.Models.DeliveryFees;
using DiamondShop.Domain.Services.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Services.Implementations
{
    public class DeliveryService : IDeliveryService
    {
        public DeliveryFee? GetDeliveryFeeForDistance(decimal distanceKilometers, List<DeliveryFee> deliveryFeesWithLocation)
        {
            foreach (var item in deliveryFeesWithLocation)
            {
                if (item.IsDistancePriceType is false)
                    continue;
                if(item.FromKm <= distanceKilometers && item.ToKm >= distanceKilometers)
                {
                    return item;
                }
            };
            return null;
        }


        public DeliveryFee? GetDeliveryFeeForLocation(Address userAddress, Address shopAddress, List<DeliveryFee> deliveryFeesWithLocation)
        {
            var userCity = userAddress.Province;// null ToLocatoin is ignored, since the list pass here is expected to have all ToLocation values
            var shopCity = shopAddress.Province;
            var correctFee = deliveryFeesWithLocation
                .Where(x => x.FromLocation != null && x.ToLocation != null)
                .FirstOrDefault(x => x.ToLocation.ToUpper() == userCity.ToUpper() && x.FromLocation.ToUpper() == shopCity.ToUpper());
            return correctFee;
        }
    }
}
