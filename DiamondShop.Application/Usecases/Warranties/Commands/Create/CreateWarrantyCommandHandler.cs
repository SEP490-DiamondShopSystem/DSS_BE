﻿using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Models.Warranties;
using DiamondShop.Domain.Models.Warranties.Enum;
using DiamondShop.Domain.Models.Warranties.ValueObjects;
using DiamondShop.Domain.Repositories;
using FluentResults;
using MediatR;

namespace DiamondShop.Application.Usecases.Warranties.Commands.Create
{
    public record CreateWarrantyCommand(WarrantyType Type, string Name, string Code, int MonthDuration, decimal Price) : IRequest<Result<Warranty>>;
    internal class CreateWarrantyCommandHandler : IRequestHandler<CreateWarrantyCommand, Result<Warranty>>
    {
        private readonly IWarrantyRepository _warrantyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateWarrantyCommandHandler(IWarrantyRepository warrantyRepository, IUnitOfWork unitOfWork)
        {
            _warrantyRepository = warrantyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Warranty>> Handle(CreateWarrantyCommand request, CancellationToken token)
        {
            request.Deconstruct(out WarrantyType type, out string name, out string code, out int duration, out decimal price);
            await _unitOfWork.BeginTransactionAsync(token);
            if (_warrantyRepository.IsNameExist(name))
                return Result.Fail("The warranty with this name has already existed");
            if (_warrantyRepository.IsCodeExist(code))
                return Result.Fail("The warranty with this code has already existed");
            Warranty warranty = Warranty.Create(type, name, code, duration, price);
            await _warrantyRepository.Create(warranty);
            await _unitOfWork.SaveChangesAsync(token);
            await _unitOfWork.CommitAsync(token);
            return warranty;
        }
    }
}