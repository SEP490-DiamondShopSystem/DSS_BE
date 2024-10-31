﻿using DiamondShop.Application.Usecases.Diamonds.Queries.GetAll;
using DiamondShop.Domain.Models.Diamonds;
using DiamondShop.Domain.Repositories.PromotionsRepo;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiamondShop.Domain.Models.DiamondPrices;

namespace DiamondShop.Application.Usecases.Diamonds.Queries.GetAllAdmin
{
    public record GetAllDiamondAdminQuery() : IRequest<List<Diamond>>;
    internal class GetAllDiamondAdminQueryHandler : IRequestHandler<GetAllDiamondAdminQuery, List<Diamond>>
    {
        private readonly IDiamondRepository _diamondRepository;
        private readonly ILogger<GetAllDiamondQueryHandler> _logger;
        private readonly IDiamondShapeRepository _diamondShapeRepository;
        private readonly IDiamondServices _diamondServices;
        private readonly IDiamondPriceRepository _diamondPriceRepository;
        private readonly IDiscountRepository _discountRepository;

        public GetAllDiamondAdminQueryHandler(IDiamondRepository diamondRepository, ILogger<GetAllDiamondQueryHandler> logger, IDiamondShapeRepository diamondShapeRepository, IDiamondServices diamondServices, IDiamondPriceRepository diamondPriceRepository, IDiscountRepository discountRepository)
        {
            _diamondRepository = diamondRepository;
            _logger = logger;
            _diamondShapeRepository = diamondShapeRepository;
            _diamondServices = diamondServices;
            _diamondPriceRepository = diamondPriceRepository;
            _discountRepository = discountRepository;
        }

        public async Task<List<Diamond>> Handle(GetAllDiamondAdminQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("get all diamond admin");
            var result = await _diamondRepository.GetAllAdmin();
            var getAllShape = await _diamondShapeRepository.GetAll();
            Dictionary<string, List<DiamondPrice>> shapeDictPrice = new();
            foreach (var shape in getAllShape)
            {
                var prices = await _diamondPriceRepository.GetPriceByShapes(shape, null, cancellationToken);
                shapeDictPrice.Add(shape.Id.Value, prices);
            }
            var getAllActiveDiscount = await _discountRepository.GetActiveDiscount();
            foreach (var diamond in result)
            {
                diamond.DiamondShape = getAllShape.FirstOrDefault(s => s.Id == diamond.DiamondShapeId);
                var diamondPrice = await _diamondServices.GetDiamondPrice(diamond, shapeDictPrice.FirstOrDefault(d => d.Key == diamond.DiamondShapeId.Value).Value);
                _diamondServices.AssignDiamondDiscount(diamond, getAllActiveDiscount).Wait();
            }
            //_diamondServices.CheckDiamondDiscount();
            return result;
        }
    }
}
