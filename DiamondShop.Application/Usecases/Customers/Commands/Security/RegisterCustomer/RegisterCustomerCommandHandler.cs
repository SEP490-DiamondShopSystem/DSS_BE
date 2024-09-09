using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using FluentResults;
using MediatR;
using System;
using System.Windows.Input;

namespace DiamondShop.Application.Usecases.Customers.Commands.Security.RegisterCustomer
{
    public record RegisterCustomerCommand(string? Email, string? Password, FullName? FullName, bool isExternalRegister = false) : IRequest<Result<Customer>>;



    internal class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, Result<Customer>>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRoleRepository _accountRoleRepository;

        public RegisterCustomerCommandHandler(IAuthenticationService authenticationService, IUnitOfWork unitOfWork, ICustomerRepository customerRepository, IAccountRoleRepository accountRoleRepository)
        {
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _accountRoleRepository = accountRoleRepository;
        }

        public async Task<Result<Customer>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
        {

            //find user role in db
            List<AccountRole> customerRoles = await _accountRoleRepository.GetRoles();
            AccountRole? customerRole = customerRoles.FirstOrDefault(c => c.Id == AccountRole.Customer.Id);
            if (customerRole is null)
                throw new ArgumentNullException("no role found");

            //start transaction
            await _unitOfWork.BeginTransactionAsync();
            Customer customer;
            if (request.isExternalRegister)
            {
                var registerResult = await _authenticationService.ExternalRegister(cancellationToken);
                if (registerResult.IsSuccess is false)
                {
                    return Result.Fail(registerResult.Errors);
                }
                string identityId = registerResult.Value.identityId;

                customer = Customer.Create(registerResult.Value.fullName, registerResult.Value.email);

                customer.SetIdentity(identityId);
            }
            else
            {
                var registerResult = await _authenticationService.Register(request.Email, request.Password, request.FullName,false, cancellationToken);
                if (registerResult.IsSuccess is false)
                {
                    return Result.Fail(registerResult.Errors);
                }
                //get identity id from Register.Result()
                string identityId = registerResult.Value;

                customer = Customer.Create(request.FullName, request.Email);

                customer.SetIdentity(identityId);
            }
            //add user to role
            customer.AddRole(customerRole);
            await _customerRepository.Create(customer);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            var getNewUser = await _customerRepository.GetById(cancellationToken, customer.Id);

            return Result.Ok(getNewUser);
        }
    }
}
