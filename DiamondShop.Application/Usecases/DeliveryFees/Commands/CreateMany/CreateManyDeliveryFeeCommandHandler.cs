﻿using DiamondShop.Application.Dtos.Requests.Deliveries;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.DeliveryFees;
using DiamondShop.Domain.Repositories.DeliveryRepo;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.DeliveryFees.Commands.CreateMany
{

    internal class CreateManyDeliveryFeeCommandHandler : IRequestHandler<CreateManyDeliveryFeeCommand, Result<List<DeliveryFee>>>
    {
        private readonly IDeliveryFeeRepository _deliveryFeeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateManyDeliveryFeeCommandHandler(IDeliveryFeeRepository deliveryFeeRepository, IUnitOfWork unitOfWork)
        {
            _deliveryFeeRepository = deliveryFeeRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<DeliveryFee>>> Handle(CreateManyDeliveryFeeCommand request, CancellationToken cancellationToken)
        {
            List<DeliveryFee> tobeAddedFees = new();
            foreach (var fee in request.fees)
            {
                DeliveryFee newFee;
                if (fee.type == DeliveryFeeType.LocationToCity)
                {
                    newFee = DeliveryFee.CreateLocationType(fee.name, fee.cost, fee.ToLocationCity!.sourceCity, fee.ToLocationCity!.destinationCity);
                }
                else if (fee.type == DeliveryFeeType.Distance)
                {
                    newFee = DeliveryFee.CreateDistanceType(fee.name, fee.cost, fee.ToDistance!.start, fee.ToDistance!.end);
                }
                else
                {
                    return Result.Fail(new ConflictError($"the fee with name: {fee.name} and cost: {fee.cost} has undefined type"));
                }
                tobeAddedFees.Add(newFee);
            }
            if (tobeAddedFees.Count == 0)
            {
                return Result.Fail(new ConflictError("No fee to be added, fill in something to add, Not a major error"));
            }
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _deliveryFeeRepository.CreateRange(tobeAddedFees, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return tobeAddedFees;
        }
    }
}
