using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;

namespace DiamondShop.Domain.Common
{
    public interface IAccountBase
    {
        public string IdentityId { get; }
        public List<AccountRole> Roles { get; }
        public FullName FullName { get; }
        public string Email { get; }
    }
}
