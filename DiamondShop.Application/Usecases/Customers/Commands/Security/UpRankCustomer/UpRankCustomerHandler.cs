using DiamondShop.Application.Services.Data;
using DiamondShop.Commons;
using DiamondShop.Domain.Models.AccountRoleAggregate.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
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

namespace DiamondShop.Application.Usecases.Customers.Commands.Security.UpRankCustomer
{
    public record UpRankCustomerCommand(CustomerId CustomerId, AccountRoleId NewRankID) : IRequest<Result>;
    public class UpRankCustomerHandler : IRequestHandler<UpRankCustomerCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRoleRepository _accountRoleRepository;

        public UpRankCustomerHandler(IUnitOfWork unitOfWork, ICustomerRepository customerRepository, IAccountRoleRepository accountRoleRepository)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _accountRoleRepository = accountRoleRepository;
        }

        public async Task<Result> Handle(UpRankCustomerCommand request, CancellationToken cancellationToken)
        {
            List<AccountRole> diamondShopCustomerRoles = await _accountRoleRepository.GetRoles();
            var getCustomerRole = diamondShopCustomerRoles.FirstOrDefault(role => role.Id == request.NewRankID);
            if (getCustomerRole == null)
                return Result.Fail(new NotFoundError("cannot found such customer role"));
            var getCustomer = await _customerRepository.GetById(cancellationToken, request.CustomerId);
            if (getCustomer == null)
                return Result.Fail(new NotFoundError("cannot found user with such id"));
            if (getCustomer.Roles.Any(r => r.Id.Value == getCustomerRole.Id.Value))
                return Result.Fail(new ConflictError("this role is already in for this user"));
            getCustomer.AddRole(getCustomerRole);
            await _unitOfWork.SaveChangesAsync();
            return Result.Ok();
        }
    }
}
