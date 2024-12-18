﻿using DiamondShop.Domain.Models.CustomizeRequests;
using DiamondShop.Domain.Models.CustomizeRequests.Enums;
using DiamondShop.Domain.Models.CustomizeRequests.ErrorMessages;
using DiamondShop.Domain.Models.CustomizeRequests.ValueObjects;
using DiamondShop.Domain.Repositories.CustomizeRequestRepo;
using FluentResults;
using MediatR;

namespace DiamondShop.Application.Usecases.CustomizeRequests.Commands.Proceed.Staff
{
    public record StaffProceedCustomizeRequestCommand(string RequestId, string? SideDiamondOptId, List<DiamondRequestAssignRecord>? DiamondAssigning) : IRequest<Result<CustomizeRequest>>;
    internal class StaffProceedCustomizeRequestCommandHandler : IRequestHandler<StaffProceedCustomizeRequestCommand, Result<CustomizeRequest>>
    {
        private readonly ICustomizeRequestRepository _customizeRequestRepository;
        private readonly ISender _sender;
        public StaffProceedCustomizeRequestCommandHandler(ICustomizeRequestRepository customizeRequestRepository, ISender sender)
        {
            _customizeRequestRepository = customizeRequestRepository;
            _sender = sender;
        }

        public async Task<Result<CustomizeRequest>> Handle(StaffProceedCustomizeRequestCommand request, CancellationToken token)
        {
            request.Deconstruct(out string requestId, out string? sideDiamondOptId, out List<DiamondRequestAssignRecord>? diamondAssigning);
            var customizeRequest = await _customizeRequestRepository.GetById(CustomizeRequestId.Parse(requestId));
            if (customizeRequest == null)
                return Result.Fail(CustomizeRequestErrors.CustomizeRequestNotFoundError);
            //if pending - assign diamond
            if (customizeRequest.ExpiredDate < DateTime.UtcNow)
                return Result.Fail(CustomizeRequestErrors.ExpiredError);
            if (customizeRequest.Status == CustomizeRequestStatus.Pending)
            {
                var result = await _sender.Send(new ProceedPendingRequestCommand(customizeRequest, sideDiamondOptId, diamondAssigning));
                if (result.IsFailed)
                    return Result.Fail(result.Errors);
                return result;
            }
            else if (customizeRequest.Status == CustomizeRequestStatus.Requesting)
            {
                var result = await _sender.Send(new ProceedRequestingRequestCommand(customizeRequest));
                if (result.IsFailed)
                    return Result.Fail(result.Errors);
                return result;
            }
            return Result.Fail(CustomizeRequestErrors.UnproceedableError);
        }
    }
}
