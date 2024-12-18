﻿using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Interfaces.JewelryModels;
using DiamondShop.Domain.Models.Jewelries.ErrorMessages;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;

namespace DiamondShop.Application.Usecases.Jewelries.Commands.Delete
{
    public record DeleteJewelryCommand(string? JewelryId) : IRequest<Result>;
    internal class DeleteJewelryCommandHandler : IRequestHandler<DeleteJewelryCommand, Result>
    {
        private readonly IJewelryRepository _jewelryRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IDiamondRepository _diamondRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJewelryService _jewelryService;

        public DeleteJewelryCommandHandler(IJewelryRepository jewelryRepository, IOrderItemRepository orderItemRepository, IUnitOfWork unitOfWork, IJewelryService jewelryService, IDiamondRepository diamondRepository)
        {
            _jewelryRepository = jewelryRepository;
            _orderItemRepository = orderItemRepository;
            _unitOfWork = unitOfWork;
            _jewelryService = jewelryService;
            _diamondRepository = diamondRepository;
        }

        public async Task<Result> Handle(DeleteJewelryCommand request, CancellationToken token)
        {
            request.Deconstruct(out string jewelryId);
            await _unitOfWork.BeginTransactionAsync(token);
            var jewelry = await _jewelryRepository.GetById(JewelryId.Parse(jewelryId));
            if (jewelry == null)
                return Result.Fail(JewelryErrors.JewelryNotFoundError);
            var isExistingFlag = await _orderItemRepository.Existing(jewelry.Id);
            if (isExistingFlag)
                return Result.Fail(JewelryErrors.JewelryInUseError);
            //Delete gallery first
            if(jewelry.Status == Domain.Common.Enums.ProductStatus.Sold)
            {
                return Result.Fail(JewelryErrors.IsSold);
            }
            foreach (var diamond in jewelry.Diamonds)
            {
                diamond.SetSell();
            }
            _diamondRepository.UpdateRange(jewelry.Diamonds);
            await _unitOfWork.SaveChangesAsync();
            await _jewelryRepository.Delete(jewelry, token);
            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            return Result.Ok();
        }
    }
}
