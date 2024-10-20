﻿using Azure.Core;
using DiamondShop.Application.Dtos.Requests.Accounts;
using DiamondShop.Application.Dtos.Responses.Accounts;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Commons;
using DiamondShop.Domain.BusinessRules;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.AccountAggregate;
using DiamondShop.Domain.Models.AccountAggregate.Entities;
using DiamondShop.Domain.Models.AccountAggregate.ValueObjects;
using DiamondShop.Domain.Repositories;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Accounts.Commands.Update
{
   
    internal class UpdateUserAccountCommandHandler : IRequestHandler<UpdateUserAccountCommand, Result<Account>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<UpdateUserAccountCommandHandler> _logger;

        public UpdateUserAccountCommandHandler(IUnitOfWork unitOfWork, IAccountRepository accountRepository, ILogger<UpdateUserAccountCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<Result<Account>> Handle(UpdateUserAccountCommand request, CancellationToken cancellationToken)
        {
            var userId = AccountId.Parse(request.userId);
            var getAccount = await _accountRepository.GetById(userId);
            if (getAccount is null)
                return Result.Fail(new NotFoundError());
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            if(request.ChangedFullName is not null)
            {
                getAccount.ChangeFullName(request.ChangedFullName);
            }
            if(request.ChangedAddress is not null)
            {
                RemoveAddress(getAccount, request.ChangedAddress);
                UpdateAddress(getAccount, request.ChangedAddress);
                AddAddress(getAccount, request.ChangedAddress);
            }
            await _accountRepository.Update(getAccount);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Ok(getAccount);
        }
        private void RemoveAddress(Account account, ChangedAddress changedAddress) 
        {
            _logger.LogInformation("Remove address function is called");
            if (changedAddress.removedAddressId is not null)
            {
                var parsedId = changedAddress.removedAddressId.Select(x => AddressId.Parse(x));
                foreach (var id in parsedId)
                    account.RemoveAddress(id);
            }
        }
        private void UpdateAddress(Account account, ChangedAddress changedAddress)
        {
            _logger.LogInformation("Update address function is called");
            if (changedAddress.updatedAddress is not null)
            {
                var parsedDict = changedAddress.updatedAddress.ToDictionary(x => AddressId.Parse(x.Key), x => x.Value);
                account.Addresses
                    .Where(ad => parsedDict.Keys.Contains(ad.Id))
                    .ToList()
                    .ForEach(ad =>
                    {
                        var address = parsedDict[ad.Id];
                        ad.Update(address.Province, address.District, address.Ward, address.Street);
                    });
            }
        }
        private void AddAddress(Account account, ChangedAddress changedAddress)
        {
            _logger.LogInformation("Add address function is called");
            if (changedAddress.addedAddress is not null)
            {
                foreach (var address in changedAddress.addedAddress)
                {
                    if (account.Addresses.Count >= AccountRules.MaxAddress)
                        return;
                    account.AddAddress(address.Province, address.District, address.Ward, address.Street);
                }
            }
        }
    }
}
