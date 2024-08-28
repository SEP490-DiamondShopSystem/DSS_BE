using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using FluentResults;
using MediatR;
using System;
using System.Windows.Input;

namespace DiamondShop.Application.Usecases.Customers.Commands.RegisterCustomer
{
    public record RegisterCustomerCommand : IRequest<Result<Customer>>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public FullName? FullName { get; set; }
    }


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
            var customer = Customer.Create(request.FullName, request.Email);
            //find user role in db
            List<DiamondShopCustomerRole> customerRoles = await _accountRoleRepository.GetCustomerRoles();
            DiamondShopCustomerRole? customerRole = customerRoles.FirstOrDefault(c => c.Id == DiamondShopCustomerRole.Customer.Id);
            if (customerRole is null)
                throw new ArgumentNullException("no role found");

            //start transaction
            await _unitOfWork.BeginTransactionAsync();
            var registerResult = await _authenticationService.Register(request.Email,request.Password,request.FullName,cancellationToken);
            if(registerResult.IsSuccess is false) 
            {
                return Result.Fail(registerResult.Errors);
            }
            //get identity id from Register.Result()
            string identityId = registerResult.Value;
            customer.SetIdentity(identityId);
            //add user to role
            customer.AddRole(customerRole);
            await _customerRepository.Create(customer);
            await _unitOfWork.SaveChangesAsync();
            //customer.AddRole(customerRole);
            //await _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            var getNewUser = await _customerRepository.GetById(cancellationToken,customer.Id);
            
            return Result.Ok(getNewUser);
        }
    }
}
