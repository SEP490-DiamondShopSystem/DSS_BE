﻿using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Usecases.PromotionRequirements.Commands.CreateMany;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.Diamonds.Enums;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using DiamondShop.Domain.Models.Promotions.Entities;
using DiamondShop.Domain.Models.Promotions.Enum;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.PromotionsRepo;
using FluentResults;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.PromotionGifts.Commands.CreateMany
{
    public record DiamondGiftSpec(DiamondOrigin Origin, string[] ShapesIDs, float caratFrom, float caratTo, Clarity clarityFrom, Clarity clarityTo, Cut cutFrom, Cut cutTo, Color colorFrom, Color colorTo);
    public record GiftSpec(string Name, TargetType TargetType, string? itemId , UnitType UnitType , decimal UnitValue, DiamondGiftSpec? DiamondRequirementSpec, int Amount = 0);
    public record CreateGiftCommand(List<GiftSpec> giftSpecs) : IRequest<Result<List<Gift>>>;
    internal class CreateGiftCommandHandler : IRequestHandler<CreateGiftCommand, Result<List<Gift>>>
    {
        private readonly IGiftRepository _giftRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IDiamondShapeRepository _diamondShapeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateGiftCommandHandler(IGiftRepository giftRepository, IPromotionRepository promotionRepository, IDiamondShapeRepository diamondShapeRepository, IUnitOfWork unitOfWork)
        {
            _giftRepository = giftRepository;
            _promotionRepository = promotionRepository;
            _diamondShapeRepository = diamondShapeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<Gift>>> Handle(CreateGiftCommand request, CancellationToken cancellationToken)
        {
            var shapes = await _diamondShapeRepository.GetAll();
            List<Gift> gifts = new();
            for (int i = 0; i < request.giftSpecs.Count; i++)
            {
                var gift = request.giftSpecs[i];
                switch(gift.TargetType) 
                {
                    case TargetType.Jewelry_Model:
                        var jewerlyModelGift = Gift.CreateJewelry(gift.Name,gift.itemId,gift.UnitType,gift.UnitValue,gift.Amount);
                        gifts.Add(jewerlyModelGift);
                        break;
                    case TargetType.Diamond:
                        gift.DiamondRequirementSpec.Deconstruct(out DiamondOrigin origin, out string[] shapesIDs, out float caratFrom, out float caratTo,
                           out Clarity clarityFrom, out var clarityTo, out Cut cutFrom, out var cutTo, out Color colorFrom, out var colorTo);
                        var selectedShape = shapes.Where(x => shapesIDs.Contains(x.Id.Value)).Select(x => x.Id).ToList();
                        var diamondGift = Gift.CreateDiamond(gift.Name, gift.itemId, gift.UnitType, gift.UnitValue, gift.Amount, selectedShape,origin,caratFrom,caratTo,clarityFrom,clarityTo,cutFrom,cutTo,colorFrom,colorTo);
                        gifts.Add(diamondGift);
                        break;
                    case TargetType.Order:
                        var orderGift = Gift.CreateOrder(gift.Name,gift.UnitType,gift.UnitValue);
                        break;
                    default:
                        return Result.Fail(new ConflictError("unspecified type found in the gift position at : " + (++i)));

                }
            }
            if (gifts.Count == 0)
                return Result.Fail(new NotFoundError("nothing to create"));
            await _giftRepository.CreateRange(gifts);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok(gifts);
            //throw new NotImplementedException();
        }
    }
    
}
