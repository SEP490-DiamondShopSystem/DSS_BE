using BeatvisionRemake.Application.Services.Interfaces;
using DiamondShop.Application.Commons.Responses;
using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Commons;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Infrastructure.Identity.Models;
using DiamondShop.Infrastructure.Options;
using DiamondShop.Infrastructure.Services;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Securities.Authentication
{
    internal class AuthenticationService : DiamondShop.Application.Services.Interfaces.IAuthenticationService
    {
        private readonly CustomRoleManager _roleManager;
        private readonly CustomSigninManager _signinManager;
        private readonly CustomUserManager _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IJwtTokenProvider _jwtTokenProvider;
        private readonly ICustomerRepository _customerRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IAccountRoleRepository _accountRoleRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IDateTimeProvider _dateTimeProvider;
        private const string BEARER_HEADER = "Bearer ";

        public AuthenticationService(CustomRoleManager roleManager, CustomSigninManager signinManager, CustomUserManager userManager, IUnitOfWork unitOfWork, ILogger<AuthenticationService> logger, IJwtTokenProvider jwtTokenProvider, ICustomerRepository customerRepository, IStaffRepository staffRepository, IAccountRoleRepository accountRoleRepository, IHttpContextAccessor contextAccessor, IDateTimeProvider dateTimeProvider)
        {
            _roleManager = roleManager;
            _signinManager = signinManager;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _jwtTokenProvider = jwtTokenProvider;
            _customerRepository = customerRepository;
            _staffRepository = staffRepository;
            _accountRoleRepository = accountRoleRepository;
            _contextAccessor = contextAccessor;
            _dateTimeProvider = dateTimeProvider;
        }



        public async Task<Result<AuthenticationResultDto>> ExternalLogin(CancellationToken cancellationToken = default)
        {
            var info = await _signinManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Result.Fail(new NotFoundError("not found user principle"));
            if ((info.Principal.Claims.FirstOrDefault(c => c.Type == "FAILURE") is not null))
                return Result.Fail("Cannot call request to user infor in google , try again later");
            var getUserByEmail = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if ( await _userManager.IsLockedOutAsync(getUserByEmail) is true)
                return Result.Fail("cannot login, you are locked out");
            if (getUserByEmail == null)
                return Result.Fail(new NotFoundError());
            var getCustomer = await _customerRepository.GetByIdentityId(getUserByEmail.Id, cancellationToken);
            if (getCustomer is null)
                return Result.Fail(new NotFoundError());
            List<AccountRole> toAccountRole = getCustomer.Roles.Select(r => (AccountRole)r).ToList();
            var authTokenDto = await GenerateTokenForUser(toAccountRole, getCustomer.Email, getCustomer.IdentityId, getCustomer.Id.Value, getCustomer.FullName.Value);
            return Result.Ok(authTokenDto);
        }

        public async Task<Result<(string identityId, FullName fullName, string email)>> ExternalRegister(CancellationToken cancellationToken = default)
        {
            var info = await _signinManager.GetExternalLoginInfoAsync();
            if (info == null)
                return Result.Fail(new NotFoundError("not found user principle"));
            if ((info.Principal.Claims.FirstOrDefault(c => c.Type == "FAILURE") is not null))
            {
                return Result.Fail("Cannot call request to user infor in google , try again later");
            }
            var tryLogin = await _signinManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            if (tryLogin.Succeeded)
                return Result.Fail(new ConflictError("user already exist"));
            var getEmail = info.Principal.FindFirstValue(ExternalAuthenticationOptions.EXTERNAL_EMAIL_CLAIM_NAME);
            var getUsername = info.Principal.FindFirstValue(ExternalAuthenticationOptions.EXTERNAL_USERNAME_CLAIM_NAME);
            var getProfileImage = info.Principal.FindFirstValue(ExternalAuthenticationOptions.EXTERNAL_PROFILE_IMAGE_CLAIM_NAME);
            ArgumentNullException.ThrowIfNull(getEmail);
            ArgumentNullException.ThrowIfNull(getUsername);
            var identity = new CustomIdentityUser()
            {
                Email = getEmail,
                UserName = getEmail,
            };
            var createResult = await _userManager.CreateAsync(identity);
            await _userManager.SetLockoutEnabledAsync(identity, false);
            if (createResult.Succeeded is false)
                return Result.Fail("fail to create identity");
            var createLogin = await _userManager.AddLoginAsync(identity, info);
            if (createLogin.Succeeded is false)
                return Result.Fail("fail to create login");
            FullName fullname;
            string[] splitedName = getUsername.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (splitedName.Length >= 2)
            {
                //string[] lastNameArray = (string[]) splitedName.Clone();
                // Array.Copy(splitedName,1,lastNameArray,0,1);
                //splitedName.CopyTo(lastNameArray, 1);lastNameArray.ToString()
                fullname = FullName.Create(splitedName[0], splitedName[1]);
            }
            else
            {
                fullname = FullName.Create(splitedName[0], "");
            }
            return (identity.Id, fullname, getEmail);
        }



        public async Task<Result<AuthenticationProperties>> GetProviderAuthProperty(string providerName, string callback_URL, CancellationToken cancellationToken = default)
        {
            var getAuthProviderSchemes = await _signinManager.GetExternalAuthenticationSchemesAsync();
            bool isSchemeExist = false;
            foreach (var authScheme in getAuthProviderSchemes)
            {
                if (string.Equals(authScheme.Name, providerName))
                {
                    isSchemeExist = true;
                    break;
                }
            }
            if (isSchemeExist is false)
            {
                return Result.Fail(new NotFoundError("not found the login requested"));
            }
            AuthenticationProperties authProperties = _signinManager.ConfigureExternalAuthenticationProperties(providerName, callback_URL);
            return Result.Ok(authProperties);
        }



        public async Task<Result<AuthenticationResultDto>> Login(string email, string password, CancellationToken cancellationToken = default)
        {
            var loginValidateResult = await PasswordLoginValidation(email, password);
            if (loginValidateResult.IsFailed)
            {
                return Result.Fail(loginValidateResult.Errors);
            }
            var userIdentity = loginValidateResult.Value;
            var getCustomer = await _customerRepository.GetByIdentityId(userIdentity.Id, cancellationToken);
            List<AccountRole> toAccountRole = getCustomer.Roles.Select(r => (AccountRole)r).ToList();
            var authTokenDto = await GenerateTokenForUser(toAccountRole, getCustomer.Email, getCustomer.IdentityId, getCustomer.Id.Value, getCustomer.FullName.Value, cancellationToken);
            return Result.Ok(authTokenDto);
        }
        private async Task<AuthenticationResultDto> GenerateTokenForUser(List<AccountRole> roles, string email, string identityId, string userId, string fullname, CancellationToken cancellationToken = default)
        {
            List<Claim> userClaim = _jwtTokenProvider.GetUserClaims(roles, email, identityId, userId, fullname);
            var accTokenResult = _jwtTokenProvider.GenerateAccessToken(userClaim);
            var refreshTokenResult = _jwtTokenProvider.GenerateRefreshToken(identityId);
            // Save Refresh Token
            await _userManager.SetRefreshTokenAsync(identityId, refreshTokenResult.refreshToken, refreshTokenResult.expiredDate, cancellationToken);
            // the inside function already have saveChanges() on db level
            //await _unitOfWork.SaveChangesAsync();
            return new AuthenticationResultDto(
                accessToken: accTokenResult.accessToken,
                expiredAccess: accTokenResult.expiredDate,
                refreshToken: refreshTokenResult.refreshToken,
                expiredRefresh: refreshTokenResult.expiredDate
            );
        }
        public async Task<Result> Logout(string identityId, CancellationToken cancellationToken = default)
        {
            await _userManager.RemoveRefreshTokenAsync(identityId, cancellationToken);
            return Result.Ok();
        }

        public async Task<Result<string>> Register(string email, string password, FullName fullname, bool emailEnabled = false, CancellationToken cancellationToken = default)
        {
            if ((await _userManager.FindByEmailAsync(email)) is not null)
            {
                return Result.Fail("User Exist");
            }
            var identity = new CustomIdentityUser();
            identity.Email = email;
            identity.UserName = email;
            identity.LockoutEnabled = false;
            identity.EmailConfirmed = emailEnabled;
            var result = await _userManager.CreateAsync(identity, password);
            await _userManager.SetLockoutEnabledAsync(identity, false);
            if (result.Succeeded is false)
            {
                var errDict = new Dictionary<string, object>();
                foreach (var err in result.Errors)
                {
                    errDict.Add(err.Code, err.Description);
                }
                return Result.Fail(new ValidationError("Password Error", errDict));
            }
            return Result.Ok(identity.Id);

        }



        public async Task<Result<AuthenticationResultDto>> LoginStaff(string email, string password, CancellationToken cancellationToken = default)
        {
            var loginValidateResult = await PasswordLoginValidation(email, password);
            if (loginValidateResult.IsFailed)
            {
                return Result.Fail(loginValidateResult.Errors);
            }
            var userIdentity = loginValidateResult.Value;
            var getStaff = await _staffRepository.GetByIdentityId(userIdentity.Id, cancellationToken);
            if (getStaff is null)
                return Result.Fail(new NotFoundError("staff not found"));
            List<AccountRole> toAccountRole = getStaff.Roles.Select(r => (AccountRole)r).ToList();
            var authTokenDto = await GenerateTokenForUser(toAccountRole, getStaff.Email, getStaff.IdentityId, getStaff.Id.value, getStaff.FullName.Value, cancellationToken);
            return Result.Ok(authTokenDto);
            // throw new NotImplementedException();
        }
        private async Task<Result<CustomIdentityUser>> PasswordLoginValidation(string email, string password)
        {
            var tryGetAccount = await _userManager.FindByEmailAsync(email);
            if (tryGetAccount is null)
            {
                return Result.Fail(new NotFoundError("account Not Found"));
            }
            if (await _userManager.CheckPasswordAsync(tryGetAccount, password) is false)
                return Result.Fail(new Error("password not match"));
            if (await _userManager.IsLockedOutAsync(tryGetAccount) is true)
                return Result.Fail("can't sign, you might have been block ");
            return Result.Ok(tryGetAccount);
        }
        public async Task<Result<(string? refreshToken, DateTime? ExpiredDate)>> GetRefreshToken(string identityId, CancellationToken cancellationToken = default)
        {
            var getIdentity = await _userManager.GetRefreshTokenAsync(identityId, cancellationToken);
            if (getIdentity.refreshToken is null)
                return Result.Fail("no token found");
            return Result.Ok((getIdentity.refreshToken, getIdentity.expiredTime));
            //throw new NotImplementedException();
        }

        public async Task<Result<ClaimsPrincipal>> GetClaimsPrincipalFromCurrentUserContext(CancellationToken cancellationToken = default)
        {
            var httpContext = _contextAccessor.HttpContext;
            var accessToken = httpContext.Request.Headers.Authorization.ToString().Substring(BEARER_HEADER.Length);
            if (accessToken.IsNullOrEmpty())
                return Result.Fail("no access token found, login to get it");
            ClaimsPrincipal? tryGetClaimFromAccessToken = _jwtTokenProvider.GetPrincipalFromExpiredToken(accessToken);
            if (tryGetClaimFromAccessToken == null)
                return Result.Fail("cannot get principle from the token, try login again");
            return Result.Ok(tryGetClaimFromAccessToken);
        }

        public async Task<Result<AuthenticationResultDto>> RefreshingToken(string refreshToken, List<AccountRole> roles, string email, string identityId, string userId, string fullname, CancellationToken cancellationToken)
        {
            var getUserRefreshTokenResul = await GetRefreshToken(identityId, cancellationToken);
            if (getUserRefreshTokenResul.IsFailed)
                return Result.Fail(getUserRefreshTokenResul.Errors);

            (string? returnedToken, DateTime? expiredTime) = getUserRefreshTokenResul.Value;
            if (refreshToken == returnedToken)
            {
                // if date time now is greater than expired time, means pass the expired or equal
                if (DateTime.Compare(_dateTimeProvider.UtcNow, expiredTime.Value) >= 0)
                {
                    Result.Fail("Token Expired, Login again");
                }
                var claims = _jwtTokenProvider.GetUserClaims(roles, email, identityId, userId, fullname);
                var newAccToken = _jwtTokenProvider.GenerateAccessToken(claims);
                var newRefreshToken = _jwtTokenProvider.GenerateRefreshToken(identityId);
                //save the new token before return it
                await _userManager.SetRefreshTokenAsync(identityId, newRefreshToken.refreshToken, newRefreshToken.expiredDate, cancellationToken);
                return Result.Ok(new AuthenticationResultDto
                (
                    accessToken: newAccToken.accessToken,
                    expiredAccess: newAccToken.expiredDate,
                    refreshToken: newRefreshToken.refreshToken,
                    expiredRefresh: newRefreshToken.expiredDate
                ));
            }
            return Result.Fail("Token not match");
        }

        public async Task<Result> BanAccount(string identityID, CancellationToken cancellationToken = default)
        {
            var tryGetAccount = await _userManager.FindByIdAsync(identityID);
            if (tryGetAccount is null)
                return Result.Fail(new NotFoundError()); ;
            tryGetAccount.LockoutEnabled = !tryGetAccount.LockoutEnabled;
            await _userManager.UpdateSecurityStampAsync(tryGetAccount);
            await _userManager.UpdateAsync(tryGetAccount);
            return Result.Ok();
        }
        public Task<Result> ConfirmEmail()
        {
            throw new NotImplementedException();
        }
        public Task<Result<string>> GenerateResetPasswordToken()
        {
            throw new NotImplementedException();
        }
        public async Task<Result> ChangePassword(string identityId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var tryGetAccount = await _userManager.FindByIdAsync(identityId);
            if (tryGetAccount is null)
                return Result.Fail(new NotFoundError()); ;
            var result = await _userManager.ChangePasswordAsync(tryGetAccount, oldPassword, newPassword);
            if (result.Succeeded is false)
            {
                Dictionary<string, object> errors = new();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Code, error.Description);
                }
                return Result.Fail(new ValidationError("password error", errors));
            }
            return Result.Ok();
        }

        public Task<Result> SendConfirmEmail()
        {
            throw new NotImplementedException();
        }
    }
}
