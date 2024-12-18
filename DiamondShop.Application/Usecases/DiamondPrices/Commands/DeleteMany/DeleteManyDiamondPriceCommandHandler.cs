﻿using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.DiamondPrices.ValueObjects;
using DiamondShop.Domain.Models.Diamonds.Enums;
using DiamondShop.Domain.Models.DiamondShapes;
using DiamondShop.Domain.Models.DiamondShapes.ErrorMessages;
using DiamondShop.Domain.Models.DiamondShapes.ValueObjects;
using DiamondShop.Domain.Repositories;
using FluentResults;
using MediatR;
using MediatR.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.DiamondPrices.Commands.DeleteMany
{

    public record DeleteDiamondPriceParameter(string criteriaId, Cut? cut, Color color, Clarity clarity);//, bool isFancy
    public record DeleteManyDiamondPriceCommand(List<string> priceIds, string? shapeId, bool isSideDiamond, bool isLab) : IRequest<Result>;
    internal class DeleteManyDiamondPriceCommandHandler : IRequestHandler<DeleteManyDiamondPriceCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiamondCriteriaRepository _diamondCriteriaRepository;
        private readonly IDiamondPriceRepository _diamondPriceRepository;
        private readonly IDiamondShapeRepository _diamondShapeRepository;

        public DeleteManyDiamondPriceCommandHandler(IUnitOfWork unitOfWork, IDiamondCriteriaRepository diamondCriteriaRepository, IDiamondPriceRepository diamondPriceRepository, IDiamondShapeRepository diamondShapeRepository)
        {
            _unitOfWork = unitOfWork;
            _diamondCriteriaRepository = diamondCriteriaRepository;
            _diamondPriceRepository = diamondPriceRepository;
            _diamondShapeRepository = diamondShapeRepository;
        }

        public async Task<Result> Handle(DeleteManyDiamondPriceCommand request, CancellationToken cancellationToken)
        {
            var getAllShape = await _diamondShapeRepository.GetAllIncludeSpecialShape();
            DiamondShape selectedShape;
            selectedShape = getAllShape.FirstOrDefault(x => x.Id == DiamondShapeId.Parse(request.shapeId));
            if (request.isSideDiamond)
                selectedShape = getAllShape.FirstOrDefault(s => s.Id == DiamondShape.ANY_SHAPES.Id);
            if (selectedShape is null)
                return Result.Fail(DiamondShapeErrors.NotFoundError);


            var parsedListIDs = request.priceIds.Select(x => DiamondPriceId.Parse(x)).ToList();
            //var parsedList = request.deleteList.Select(x => new DeleteManyParameter
            //(
            //    selectedShape.Id,
            //    DiamondCriteriaId.Parse(x.criteriaId)
            //)).ToList();
            var deleteResult = await _diamondPriceRepository.DeleteMany(parsedListIDs, selectedShape, request.isLab, request.isSideDiamond, cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            _diamondPriceRepository.RemoveAllCache();
            return deleteResult;

        }
    }
}
