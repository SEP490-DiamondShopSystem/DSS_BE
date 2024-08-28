using BeatvisionRemake.Application.Services.Interfaces;
using BeatvisionRemake.Domain.Common;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BeatvisionRemake.Infrastructure.Services
{
    internal class JwtTokenProvider : IJwtTokenProvider
    {
        // public const string JWT_SINGING_KEY= "adfsasdfadsfasdfasdfasdfsadfsdfdasffasdds";
        private readonly JwtOptions _jwtOptions;
        private readonly JwtBearerOptions _jwtBearerOptions;
        public JwtTokenProvider(IOptions<JwtOptions> jwtOptions, IOptions<JwtBearerOptions> jwtBearerOptions)
        {
            _jwtOptions = jwtOptions.Value;
            _jwtBearerOptions = jwtBearerOptions.Value;
        }

        public (string accessToken, DateTime expiredDate) GenerateAccessToken(List<Claim> claims)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var expiredTime = DateTime.Now.AddMinutes(100);
            var tokeOptions = new JwtSecurityToken(
                claims: claims,
                expires: expiredTime,
                signingCredentials: signinCredentials
            ); ;

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return (tokenString, expiredTime);
        }

        public (string refreshToken, DateTime expiredDate) GenerateRefreshToken(string identityId)
        {
            return (Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                DateTime.Now.AddHours(7));
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            //var tokenValidationParameters = new TokenValidationParameters
            //{
            //    ValidateIssuerSigningKey = true,
            //    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            //    ValidateLifetime = false, //here we are saying that we don't care about the token's expiration date
            //};
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(token, _jwtBearerOptions.TokenValidationParameters, out securityToken);
            }
            catch (SecurityTokenValidationException ex)
            {
                return null;
            }
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public List<Claim> GetUserClaims(List<AccountRole> roles, string email, string id)
        {
            var claims = new List<Claim>();
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Id.ToString()) );
            }
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Name, id));

            return claims;
        }
    }
}
