using DiamondShop.Application.Services.Data;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Customers.Commands.Security.DownRankCustomer
{
    public record DownRankCustomerCommand(CustomerId CustomerId, AccountRoleId RankToRemoveID) : IRequest<Result>;
    public class DownRankCustomerHandler : IRequestHandler<DownRankCustomerCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRoleRepository _accountRoleRepository;

        public DownRankCustomerHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository, IAccountRoleRepository accountRoleRepository)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _accountRoleRepository = accountRoleRepository;
        }

        public async Task<Result> Handle(DownRankCustomerCommand request, CancellationToken cancellationToken)
        {
            var getCustomer = await _customerRepository.GetById(cancellationToken, request.CustomerId);
            if (getCustomer == null)
                return Result.Fail(new NotFoundError("cannot found user with such id"));
            AccountRole? userRoleToRemove = getCustomer.Roles.FirstOrDefault(r => r.Id == request.RankToRemoveID);
            if (userRoleToRemove == null)
                return Result.Fail(new NotFoundError("cannot found such role in user"));
            getCustomer.RemoveRole(userRoleToRemove);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok();
        }
    }
}
