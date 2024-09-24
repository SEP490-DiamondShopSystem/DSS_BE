﻿using DiamondShop.Application.Services.Data;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.Diamonds.ValueObjects;
using DiamondShop.Domain.Repositories;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Diamonds.Commands.Delete
{
    public record DeleteDiamondCommand(string diamondId) : IRequest<Result>;
    internal class DeleteDiamondCommandHandler : IRequestHandler<DeleteDiamondCommand,Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiamondRepository _diamondRepository;

        public DeleteDiamondCommandHandler(IUnitOfWork unitOfWork, IDiamondRepository diamondRepository)
        {
            _unitOfWork = unitOfWork;
            _diamondRepository = diamondRepository;
        }

        public async Task<Result> Handle(DeleteDiamondCommand request, CancellationToken cancellationToken)
        {
            var diamondId = DiamondId.Parse(request.diamondId);
            var getDiamond = await _diamondRepository.GetById(diamondId);
            if (getDiamond == null)
                return Result.Fail(new NotFoundError("not found this diamond"));
            if (getDiamond.Warranty is not null)
                return Result.Fail(new ConflictError("this diamond seems to have a warranty set to it, so it is bought already, cannot delelte"));
            if(getDiamond.JewelryId is not null)
                return Result.Fail(new ConflictError("this diamond seems to have a jewelry set to it, so it is used already, cannot delelte"));
            await _diamondRepository.Delete(getDiamond);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok();
        }
    }
}