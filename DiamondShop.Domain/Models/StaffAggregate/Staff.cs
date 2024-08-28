using BeatvisionRemake.Domain.Common;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate.ValueObjects;
using DiamondShop.Domain.Roles;

namespace DiamondShop.Domain.Models.StaffAggregate
{
    public class Staff : Entity<StaffId>, IAggregateRoot
    {
        private Staff()
        {

        }
        internal Staff(StaffId staffId, FullName fullName, string departmentLocation, string email) : base(staffId)
        {
            FullName = fullName;
            DepartmentLocation = departmentLocation;
            Email = email;
        }
        public string IdentityId { get; private set; }
        public List<DiamondShopStoreRoles> Roles { get; private set; } = new();
        public FullName FullName { get; private set; }
        public string Email { get; private set; }
        public string DepartmentLocation { get; private set; }
        public static Staff Create(string identity, FullName fullName, string email)
        {
            var user = new Staff(StaffId.Create(),fullName,"NONE", email);
            user.SetIdentity(identity);
            return user;
        }
        private void SetIdentity(string identityID)
        {
            IdentityId = identityID;

        }
        public void AddRole(DiamondShopStoreRoles role)
        {
            Roles.Add(role);
        }
        public void RemoveRole(DiamondShopStoreRoles role)
        {
            Roles.Remove(role);
        }
    }
}
