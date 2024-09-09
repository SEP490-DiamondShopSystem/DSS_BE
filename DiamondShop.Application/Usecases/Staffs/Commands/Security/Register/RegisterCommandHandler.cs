using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Usecases.Staffs.Commands.Security.Register
{
    public record RegisterCommand(string email, string password, FullName fullName , bool isManager) : IRequest<Result<Staff>>;
    internal class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<Staff>>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStaffRepository _staffRepository;
        private readonly IDateTimeProvider _timeProvider;
        private readonly IAccountRoleRepository _accountRoleRepository;

        public RegisterCommandHandler(IAuthenticationService authenticationService, IUnitOfWork unitOfWork, IStaffRepository staffRepository, IDateTimeProvider timeProvider, IAccountRoleRepository accountRoleRepository)
        {
            _authenticationService = authenticationService;
            _unitOfWork = unitOfWork;
            _staffRepository = staffRepository;
            _timeProvider = timeProvider;
            _accountRoleRepository = accountRoleRepository;
        }

        public async Task<Result<Staff>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            List<AccountRole> storeRoles = await _accountRoleRepository.GetRoles();
            AccountRole staffRole = storeRoles.FirstOrDefault(c => c.Id == AccountRole.Staff.Id);
            //start transaction
            await _unitOfWork.BeginTransactionAsync();
            var identityResult = await _authenticationService.Register(request.email,request.password,request.fullName, true,cancellationToken);
            if (identityResult.IsSuccess is false)
                return Result.Fail(identityResult.Errors);
            string identityId = identityResult.Value;
            Staff staff = Staff.Create(identityId,request.fullName,request.email);
            staff.AddRole(staffRole);
            if (request.isManager)
            {
                AccountRole managerRole = storeRoles.FirstOrDefault(c => c.Id == AccountRole.Manager.Id);
                staff.AddRole(managerRole);   
            }
            await _staffRepository.Create(staff);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            Staff getStaff = await _staffRepository.GetById(cancellationToken,staff.Id);
            return Result.Ok(getStaff);
        }
    }
}
