﻿using DiamondShop.Domain.Models.AccountAggregate.Entities;
using DiamondShop.Domain.Models.DeliveryFees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Domain.Services.interfaces
{
    public interface IDeliveryService
    {
        DeliveryFee? GetDeliveryFeeForLocation(Address userAddress, List<DeliveryFee> deliveryFeesWithLocation);
    }
}