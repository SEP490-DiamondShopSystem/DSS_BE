
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.RoleAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BeatvisionRemake.Application.Services.Interfaces
{
    public interface IJwtTokenProvider
    {
        (string accessToken, DateTime expiredDate) GenerateAccessToken(List<Claim> claims);
        (string refreshToken, DateTime expiredDate) GenerateRefreshToken(string identityId);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        List<Claim> GetUserClaims(List<AccountRole> roles, string email, string identityId);
    }
}
