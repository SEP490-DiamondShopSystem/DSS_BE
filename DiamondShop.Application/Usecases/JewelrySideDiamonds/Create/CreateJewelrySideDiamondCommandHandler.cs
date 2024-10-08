﻿using DiamondShop.Application.Services.Data;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.Jewelries.Entities;
using DiamondShop.Domain.Models.Jewelries.ValueObjects;
using DiamondShop.Domain.Models.JewelryModels.Entities;
using DiamondShop.Domain.Models.JewelryModels.ValueObjects;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using DiamondShop.Domain.Repositories.JewelryRepo;
using FluentResults;
using MediatR;

namespace DiamondShop.Application.Usecases.JewelrySideDiamonds.Create
{
    public record CreateJewelrySideDiamondCommand(JewelryId JewelryId, List<SideDiamondOptId> SideDiamondOptIds) : IRequest<Result>;
    internal class CreateJewelrySideDiamondCommandHandler : IRequestHandler<CreateJewelrySideDiamondCommand, Result>
    {
        private readonly IJewelrySideDiamondRepository _jewelrySideDiamondRepository;
        private readonly ISideDiamondRepository _sideDiamondRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateJewelrySideDiamondCommandHandler(IJewelrySideDiamondRepository jewelrySideDiamondRepository, IUnitOfWork unitOfWork)
        {
            _jewelrySideDiamondRepository = jewelrySideDiamondRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CreateJewelrySideDiamondCommand request, CancellationToken token)
        {
            await _unitOfWork.BeginTransactionAsync(token);
            request.Deconstruct(out JewelryId jewelryId, out List<SideDiamondOptId> sideDiamondOptId);
            var modelSideDiamonds = await _sideDiamondRepository.GetSideDiamondOption(sideDiamondOptId);
            if (modelSideDiamonds.Count != sideDiamondOptId.Count) return Result.Fail(new ConflictError("Can't find this side diamond option"));

            var flagDuplicateSideDiamond = CheckDuplicateSideDiamond(modelSideDiamonds);
            if (flagDuplicateSideDiamond) return Result.Fail(new ConflictError("Duplicated jewelry side diamond"));

            List<JewelrySideDiamond> sideDiamonds = modelSideDiamonds.Select(p => JewelrySideDiamond.Create(jewelryId, p)).ToList();
            await _jewelrySideDiamondRepository.CreateRange(sideDiamonds);
            await _unitOfWork.SaveChangesAsync(token);
            return Result.Ok();
        }
        public bool CheckDuplicateSideDiamond(List<SideDiamondOpt> sideDiamonds)
        {
            List<SideDiamondReqId> exists = new();
            foreach (var side in sideDiamonds)
            {
                if (exists.Contains(side.SideDiamondReqId)) return true;
                exists.Add(side.SideDiamondReqId);
            }
            return false;
        }
    }
}