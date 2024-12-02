﻿using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Commons;
using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Models.DiamondPrices;
using DiamondShop.Domain.Models.DiamondPrices.ValueObjects;
using DiamondShop.Domain.Models.DiamondShapes;
using DiamondShop.Domain.Models.DiamondShapes.ErrorMessages;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.DiamondPrices.Commands.UpdateMany
{
    public record UpdatedDiamondPrice(string diamondPriceId, decimal price);//bool isFancyShapePrice
    public record UpdateManyDiamondPricesCommand(List<UpdatedDiamondPrice> updatedDiamondPrices, string? shapeId , bool isLabDiamond, bool IsSideDiamond) : IRequest<Result<List<DiamondPrice>>>;
    internal class UpdateManyDiamondPricesCommandHandler : IRequestHandler<UpdateManyDiamondPricesCommand, Result<List<DiamondPrice>>>
    {
        private readonly IDiamondPriceRepository _diamondPriceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiamondCriteriaRepository _diamondCriteriaRepository;
        private readonly IDiamondShapeRepository _diamondShapeRepository;
        private readonly IDiamondServices _diamondServices;

        public UpdateManyDiamondPricesCommandHandler(IDiamondPriceRepository diamondPriceRepository, IUnitOfWork unitOfWork, IDiamondCriteriaRepository diamondCriteriaRepository, IDiamondShapeRepository diamondShapeRepository, IDiamondServices diamondServices)
        {
            _diamondPriceRepository = diamondPriceRepository;
            _unitOfWork = unitOfWork;
            _diamondCriteriaRepository = diamondCriteriaRepository;
            _diamondShapeRepository = diamondShapeRepository;
            _diamondServices = diamondServices;
        }

        public async Task<Result<List<DiamondPrice>>> Handle(UpdateManyDiamondPricesCommand request, CancellationToken cancellationToken)
        {
            var getAllShape = await _diamondShapeRepository.GetAllIncludeSpecialShape();
            var parsedId = DiamondShapeId.Parse(request.shapeId);
            DiamondShape selectedShape = getAllShape.FirstOrDefault(x => x.Id == parsedId);
            if (request.IsSideDiamond)
                selectedShape = getAllShape.FirstOrDefault(s => s.Id == DiamondShape.ANY_SHAPES.Id);
            if (selectedShape is null)
                return Result.Fail(DiamondShapeErrors.NotFoundError);



            List<(DiamondPriceId criteriaId, decimal normalizedPrice)> parsedList =
                request.updatedDiamondPrices
                .Select(x => (DiamondPriceId.Parse(x.diamondPriceId), MoneyVndRoundUpRules.RoundAmountFromDecimal(x.price))
                ).ToList();
            List<DiamondPriceId> priceIds = parsedList.Select(x => x.criteriaId).ToList();
            List<DiamondPrice> getPrices = new();
            bool isFancyShape = DiamondShape.IsFancyShape(selectedShape.Id);

            if (request.IsSideDiamond is false)
                getPrices = await _diamondPriceRepository.GetPriceIgnoreCache(selectedShape, request.isLabDiamond, cancellationToken);
            else
            {
                //var isFancyShape = DiamondShape.IsFancyShape(selectedShape.Id);
                getPrices = await _diamondPriceRepository.GetSideDiamondPrice( request.isLabDiamond, cancellationToken);
            }
            
            var getPriceByIds = getPrices.Where(x => priceIds.Contains(x.Id)).ToList();
            foreach (var price in getPriceByIds)
            {
                var updatedPrice = parsedList.FirstOrDefault(x => x.criteriaId == price.Id);
                price.ChangePrice(updatedPrice.normalizedPrice);
                await _diamondPriceRepository.Update(price);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new List<DiamondPrice>();



            ////var parsedShapeId = DiamondShapeId.Parse(request.);
            ////var getShape = await _diamondShapeRepository.GetById(parsedShapeId);
            ////if (getShape == null)
            ////    return Result.Fail(new NotFoundError());
            ////if (getShape.Id != DiamondShape.ROUND.Id && getShape.Id != DiamondShape.FANCY_SHAPES.Id)
            ////    return Result.Fail("the shape for price can only be Round brilliant or Fancy, which is round and the rest of the shape");
            //var getAllShape = await _diamondShapeRepository.GetAllIncludeSpecialShape();
            //DiamondShape selectedShape;
            ////if (request.IsSideDiamond == false )
            ////{
            //if (request.isFancyShapePrice == null)
            //    return Result.Fail("main diamond must have is fancy shape");
            //if (request.isFancyShapePrice)
            //    selectedShape = getAllShape.FirstOrDefault(x => x.Id == DiamondShape.FANCY_SHAPES.Id);
            //else
            //    selectedShape = getAllShape.FirstOrDefault(x => x.Id == DiamondShape.ROUND.Id);
            ////}
            ////else
            ////{
            ////selectedShape = getAllShape.FirstOrDefault(x => x.Id == DiamondShape.ANY_SHAPES.Id);
            ////}

            //List<(DiamondCriteriaId criteriaId, decimal normalizedPrice)> parsedList =
            //    request.updatedDiamondPrices
            //    .Select(x => (DiamondCriteriaId.Parse(x.diamondCriteriaId), MoneyVndRoundUpRules.RoundAmountFromDecimal(x.price))
            //    ).ToList();
            //List<DiamondCriteriaId> diamondCriteriaIds = parsedList.Select(x => x.criteriaId).ToList();
            //List<DiamondPrice> getPrices = new();
            //bool isFancyShape = DiamondShape.IsFancyShape(selectedShape.Id);

            //if (request.IsSideDiamond is false)
            //    getPrices = await _diamondPriceRepository.GetPriceIgnoreCache(isFancyShape, request.isLabDiamond, cancellationToken);
            //else
            //    getPrices = await _diamondPriceRepository.GetSideDiamondPrice(request.isFancyShapePrice,request.isLabDiamond,cancellationToken);
            ////if(request.IsSideDiamond is false)
            ////    getPrices = await _diamondPriceRepository.GetPriceByShapes(selectedShape,request.islabDiamond,cancellationToken);
            ////else
            ////    getPrices = await _diamondPriceRepository.GetSideDiamondPrice( request.islabDiamond, cancellationToken);

            //var getPriceByCriteria = getPrices.Where(x => diamondCriteriaIds.Contains(x.CriteriaId)).ToList();
            //foreach (var price in getPriceByCriteria)
            //{
            //    var updatedPrice = parsedList.FirstOrDefault(x => x.criteriaId == price.CriteriaId);
            //    price.ChangePrice(updatedPrice.normalizedPrice);
            //    await _diamondPriceRepository.Update(price);
            //}
            //await _unitOfWork.SaveChangesAsync(cancellationToken);
            //return new List<DiamondPrice>();
        }
    }
}
