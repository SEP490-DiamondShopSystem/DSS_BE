
using DiamondShop.Application.Commons.Responses;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using FluentResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Application.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Result<string>> Register(string email, string password, FullName fullname, CancellationToken cancellationToken = default);
        Task<Result<AuthenticationResultDto>> Login(string email, string password, CancellationToken cancellationToken = default);
        Task<Result> Logout(string identityID, CancellationToken cancellationToken = default);
        //Task<Result<IUserIdentity>> GetUserIdentity(string identityId, CancellationToken cancellationToken = default);
        Task<Result> ConfirmEmail();
        Task<Result> SendConfirmEmail();
        Task<Result> ResetPassword();
        Task<Result<string>> GenerateResetPasswordToken();

    }
}
