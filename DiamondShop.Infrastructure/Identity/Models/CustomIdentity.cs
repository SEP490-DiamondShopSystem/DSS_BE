using DiamondShop.Domain.Common;
using DiamondShop.Domain.Common.ValueObjects;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DiamondShop.Infrastructure.Identity.Models
{
    public class CustomIdentityUser : IdentityUser<string> 
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override string Id { get; set; } = Guid.NewGuid().ToString();    
        public CustomIdentityUser()
        {
            SecurityStamp = Guid.NewGuid().ToString();
        }
        public CustomIdentityUser(string userName) : this()
        {
            UserName = userName;
        }
        public DateTime? Dob { get; set; }
        public virtual List<CustomIdentityUserToken> UserTokens { get; set; } = new();
        public virtual List<CustomIdentityUserClaims> UserClaims { get; set; } = new();
        public virtual List<CustomIdentityRole> UserRoles { get; set; } = new();
        public virtual List<CustomIdentityUserLogins> UserLogins { get; set; } = new();


    }
    public class CustomIdentityRole : IdentityRole<string>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override string Id { get; set; }
        public string? Description { get; set; }
        public CustomIdentityRole()
        {
        }
        public CustomIdentityRole(string roleName) : this()
        {
            Name = roleName;
        }
        public CustomIdentityRole(string id, string roleName) : this()
        {
            Id = id;
            Name = roleName;
        }
        public virtual IList<CustomIdentityUser> Users { get; set; } = new List<CustomIdentityUser>();
        public virtual IList<CustomIdentityUserRoleClaim> RoleClaims { get; set; } = new List<CustomIdentityUserRoleClaim>();
    }

    public class CustomIdentityUserRoleClaim : IdentityRoleClaim<string>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
		public virtual CustomIdentityRole? Role { get; set; }

        public CustomIdentityUserRoleClaim()
        {
        }
    }

    public class CustomIdentityUserRole : IdentityUserRole<string>
    {
        public CustomIdentityUserRole()
        {

        }
    }

    public class CustomIdentityUserClaims : IdentityUserClaim<string>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }
        public CustomIdentityUserClaims()
        {

        }
    }

    public class CustomIdentityUserLogins : IdentityUserLogin<string>
    {
        public CustomIdentityUserLogins()
        {

        }
    }

    public class CustomIdentityUserToken : IdentityUserToken<string>
    {
        public CustomIdentityUserToken() 
        {

        }
        public DateTime? ExpiredDate { get; set; }
    }
}
