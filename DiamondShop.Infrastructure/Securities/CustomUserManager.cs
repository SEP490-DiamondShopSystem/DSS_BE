using DiamondShop.Domain.Common;
using DiamondShop.Infrastructure.Databases;
using DiamondShop.Infrastructure.Identity.Models;
using DiamondShop.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiamondShop.Infrastructure.Securities
{
    public class CustomUserManager : UserManager<CustomIdentityUser>
    {
        private readonly JwtOptions _jwtOptions;
        public const string REFRESH_TOKEN = "RefreshToken";
        private readonly DiamondShopDbContext _diamondShopDbContext;

        public CustomUserManager(IUserStore<CustomIdentityUser> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<CustomIdentityUser> passwordHasher,
            IEnumerable<IUserValidator<CustomIdentityUser>> userValidators,
            IEnumerable<IPasswordValidator<CustomIdentityUser>> passwordValidators,
            ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<CustomIdentityUser>> logger,
            IOptions<JwtOptions> jwtOptions,
            DiamondShopDbContext dbContext) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _jwtOptions = jwtOptions.Value;
            _diamondShopDbContext = dbContext;
        }
        public async Task<IdentityResult> SetRefreshTokenAsync(string identityId, string? tokenValue, DateTime expiredDate, CancellationToken cancellationToken = default)
        {
            
            ThrowIfDisposed();
            var loginProvider = _jwtOptions.ValidIssuer;
            ArgumentNullException.ThrowIfNull(tokenValue);
            ArgumentNullException.ThrowIfNull(identityId);
            ArgumentNullException.ThrowIfNull(loginProvider);
            var tryGetOldRefreshToken = await _diamondShopDbContext.UserTokens.FirstOrDefaultAsync(t => t.UserId == identityId && t.Name == REFRESH_TOKEN);
            if (tryGetOldRefreshToken != null)
            {
                _diamondShopDbContext.UserTokens.Remove(tryGetOldRefreshToken);
            }
            _diamondShopDbContext.UserTokens.Add(new CustomIdentityUserToken
            {
                ExpiredDate = expiredDate,
                LoginProvider = loginProvider,
                Name = REFRESH_TOKEN,
                UserId = identityId,
                Value = tokenValue,
            });
            _diamondShopDbContext.SaveChanges();
            return IdentityResult.Success;

        }
        public async Task<IdentityResult> RemoveRefreshTokenAsync(string identityId, CancellationToken cancellationToken =default)
        {
            ThrowIfDisposed();
            var loginProvider = _jwtOptions.ValidIssuer;
            ArgumentNullException.ThrowIfNull(identityId);
            ArgumentNullException.ThrowIfNull(loginProvider);
            var tryGetOldRefreshToken = await _diamondShopDbContext.UserTokens.FirstOrDefaultAsync(t => t.UserId == identityId && t.Name == REFRESH_TOKEN);
            if (tryGetOldRefreshToken != null)
            {
                _diamondShopDbContext.UserTokens.Remove(tryGetOldRefreshToken);
                _diamondShopDbContext.SaveChanges();
            }
            return IdentityResult.Success;
        }
    }
}
