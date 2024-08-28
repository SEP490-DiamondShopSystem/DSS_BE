using BeatvisionRemake.Application.Services.Interfaces;
using DiamondShop.Application.Commons.Responses;
using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Commons;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Infrastructure.Identity.Models;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Securities.Authentication
{
    internal class AuthenticationService : IAuthenticationService
    {
        private readonly CustomRoleManager _roleManager;
        private readonly CustomSigninManager _signinManager;
        private readonly CustomUserManager _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly ICustomerRepository _customerRepository;
        private readonly IAccountRoleRepository _accountRoleRepository;

        public AuthenticationService(CustomRoleManager roleManager, CustomSigninManager signinManager, CustomUserManager userManager, IUnitOfWork unitOfWork, ILogger<AuthenticationService> logger, IJwtTokenProvider jwtTokenProvider, ICustomerRepository customerRepository, IAccountRoleRepository accountRoleRepository)
        {
            _roleManager = roleManager;
            _signinManager = signinManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtTokenProvider = jwtTokenProvider;
            _customerRepository = customerRepository;
            _accountRoleRepository = accountRoleRepository;
        }

        public Task<Result> ConfirmEmail()
        {
            throw new NotImplementedException();
        }

        public Task<Result<string>> GenerateResetPasswordToken()
        {
            throw new NotImplementedException();
        }


        public async Task<Result<AuthenticationResultDto>> Login(string email, string password, CancellationToken cancellationToken = default)
        {
            var tryGetUser = await _userManager.FindByEmailAsync(email);
            if (tryGetUser is null)
            {
                return Result.Fail(new NotFoundError("User Not Found"));
            }
            var getCustomer = await _customerRepository.GetByIdentityId(tryGetUser.Id,cancellationToken);
            try
            {
                List<AccountRole> toAccountRole = getCustomer.Roles.Select(r => (AccountRole)r).ToList();
                List<Claim> userClaim = _jwtTokenProvider.GetUserClaims(toAccountRole, getCustomer.Email,getCustomer.IdentityId);
                var accTokenResult = _jwtTokenProvider.GenerateAccessToken(userClaim);
                var refreshTokenResult = _jwtTokenProvider.GenerateRefreshToken(getCustomer.IdentityId);

                return Result.Ok(new AuthenticationResultDto(
                    accessToken: accTokenResult.accessToken,
                    expiredAccess: accTokenResult.expiredDate,
                    refreshToken: refreshTokenResult.refreshToken,
                    expiredRefresh: accTokenResult.expiredDate
                ));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Result> Logout(string identityId, CancellationToken cancellationToken = default)
        {
            await _userManager.RemoveRefreshTokenAsync(identityId, cancellationToken);
            return Result.Ok();
        }

        public async Task<Result<string>> Register(string email, string password, FullName fullname, CancellationToken cancellationToken = default)
        {
            if ((await _userManager.FindByEmailAsync(email)) is not null)
            {
                return Result.Fail("User Exist");
            }
            try
            {
                var identity = new CustomIdentityUser();
                identity.Email = email;
                identity.UserName = email;
                identity.LockoutEnabled = false;
                var result = await _userManager.CreateAsync(identity, password);
                if (result.Succeeded is false)
                {
                    var errDict = new Dictionary<string, object>();
                    foreach(var err in result.Errors)
                    {
                        errDict.Add(err.Code, err.Description);
                    }
                    return Result.Fail(new ValidationError("Password Error", errDict));
                }
                return Result.Ok(identity.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public Task<Result> ResetPassword()
        {
            throw new NotImplementedException();
        }

        public Task<Result> SendConfirmEmail()
        {
            throw new NotImplementedException();
        }
    }
}
