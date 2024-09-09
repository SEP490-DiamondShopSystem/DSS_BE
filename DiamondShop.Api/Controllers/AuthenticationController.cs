using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Usecases.Customers.Commands.Security.Login;
using DiamondShop.Application.Usecases.Customers.Commands.Security.RegisterCustomer;
using DiamondShop.Application.Usecases.Staffs.Commands.Security.BanAccount;
using DiamondShop.Application.Usecases.Staffs.Commands.Security.Login;
using DiamondShop.Application.Usecases.Staffs.Commands.Security.Register;
using DiamondShop.Application.Usecases.Staffs.Commands.Security.RegisterAdmin;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Roles;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace DiamondShop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ApiControllerBase
    {
        private readonly ISender _sender;
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(ISender sender, IAuthenticationService authenticationService)
        {
            _sender = sender;
            _authenticationService = authenticationService;
        }

        [HttpPost("Register")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public async Task<ActionResult> Register([FromBody] RegisterCustomerCommand? registerCustomerCommand, [FromQuery] string? externalProviderName, CancellationToken cancellationToken = default)
        {
            if (externalProviderName == null && registerCustomerCommand is not null)
            {
                var command = registerCustomerCommand with { isExternalRegister = false };
                var result = await _sender.Send(command);
                if (result.IsSuccess is false)
                    return MatchError(result.Errors, ModelState);
                return Ok(result.Value);
            }
            else
            {
                var result = await _authenticationService.GetProviderAuthProperty(externalProviderName, Url.Action(nameof(ExternalRegisterCallback)));
                if (result.IsSuccess is false)
                {
                    return MatchError(result.Errors, ModelState);
                }
                Microsoft.AspNetCore.Authentication.AuthenticationProperties authenticationProperties = result.Value;
                return new ChallengeResult(externalProviderName, authenticationProperties);
            }

        }
        [HttpPost("Login")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public async Task<ActionResult> Login([FromBody] LoginCustomerCommand? loginCustomerQuery, [FromQuery] string? externalProviderName, CancellationToken cancellationToken = default)
        {
            if (externalProviderName == null && loginCustomerQuery is not null)
            {
                var command = loginCustomerQuery with { isExternalLogin = false };
                var result = await _sender.Send(command);
                if (result.IsSuccess is false)
                    return MatchError(result.Errors, ModelState);
                return Ok(result.Value);
            }
            else
            {
                var result = await _authenticationService.GetProviderAuthProperty(externalProviderName, Url.Action(nameof(ExternalLoginCallback)));
                Microsoft.AspNetCore.Authentication.AuthenticationProperties authenticationProperties = result.Value;
                return new ChallengeResult(externalProviderName, authenticationProperties);
            }
        }
        [HttpPost("LoginStaff")]
        [Consumes("application/json")]
        [AllowAnonymous]
        public async Task<ActionResult> LoginStaff([FromBody] LoginCommand loginCommand, CancellationToken cancellationToken = default)
        {
                var result = await _sender.Send(loginCommand);
                if (result.IsSuccess is false)
                    return MatchError(result.Errors, ModelState);
                return Ok(result.Value);
        }
        [HttpPost("RegisterStaff")]
        [Consumes("application/json")]
        public async Task<ActionResult> RegisterStaff([FromBody] RegisterCommand registerCommand, CancellationToken cancellationToken = default)
        {
                var result = await _sender.Send(registerCommand);
                if (result.IsSuccess is false)
                    return MatchError(result.Errors, ModelState);
                return Ok(result.Value);
        }
        [HttpPost("RegisterAdmin")]
        [Consumes("application/json")]
        public async Task<ActionResult> RegisterAdmin([FromBody] RegisterAdminCommand registerAdminCommand, CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(registerAdminCommand);
            if (result.IsSuccess is false)
                return MatchError(result.Errors, ModelState);
            return Ok(result.Value);
        }
        [HttpGet("external-register-callback-url")]
        public async Task<ActionResult> ExternalRegisterCallback()
        {
            //this command is mock up for it to pass the validator
            var result = await _sender.Send(new RegisterCustomerCommand("fake@gmail.com", "123", FullName.Create("abc", "abc"), isExternalRegister: true));
            if (result.IsSuccess is false)
                return MatchError(result.Errors, ModelState);
            return Ok(result.Value);
        }
        [HttpGet("external-Login-callback-url")]
        public async Task<ActionResult> ExternalLoginCallback()
        {
            //this command is mock up for it to pass the validator
            var result = await _sender.Send(new LoginCustomerCommand("123132", "123123", true));
            if (result.IsSuccess is false)
                return MatchError(result.Errors, ModelState);
            return Ok(result.Value);
        }
        [HttpPut("CustomerRefresh")]
        [Authorize(Roles = AccountRole.CustomerId)]
        public async Task<ActionResult> CustomerRefreshingToken([FromQuery] string refreshToken, CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(new DiamondShop.Application.Usecases.Customers.Commands.Security.RefreshToken.RefreshTokenCommand(refreshToken));
            if (result.IsSuccess is false)
                return MatchError(result.Errors, ModelState);
            return Ok(result.Value);
        }
        
        [HttpPut("StaffRefresh")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> StaffRefreshingToken([FromQuery] string refreshToken, CancellationToken cancellationToken = default)
        {
            var result = await _sender.Send(new DiamondShop.Application.Usecases.Staffs.Commands.Security.RefreshToken.RefreshTokenCommand(refreshToken));

            if (result.IsSuccess is false)
                return MatchError(result.Errors, ModelState);
            return Ok(result.Value);
        }
        [HttpPut("Ban")]
        [Authorize(Roles = AccountRole.StaffId)]
        public async Task<ActionResult> BanAccount([FromQuery]string identityId , CancellationToken cancellationToken =default)
        {
            var result = await _sender.Send(new BanAccountCommand(identityId));
            if (result.IsSuccess is false)
                return MatchError(result.Errors, ModelState);
            return Ok();
        }
    }
}
