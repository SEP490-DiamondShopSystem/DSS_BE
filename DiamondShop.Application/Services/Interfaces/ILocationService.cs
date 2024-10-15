﻿using DiamondShop.Application.Services.Models;
using DiamondShop.Domain.Common.Addresses;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Services.Interfaces
{
    public interface ILocationService
    {
        List<Province> GetProvinces();
        List<District> GetDistricts( string provinceId);
        List<Ward> GetWards(string districtId);

        Task<Result<LocationDistantData>> GetDistant(LocationDetail Origin, LocationDetail Destination, CancellationToken cancellationToken = default); 
    }
}
