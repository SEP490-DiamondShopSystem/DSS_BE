using DiamondShop.Application.Services.Data;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using DiamondShop.Infrastructure.Databases.Configurations;
using DiamondShop.Infrastructure.Securities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases
{
    public class Seeding
    {
        public static async Task SeedAsync(IAccountRoleRepository accountRoleRepository, IUnitOfWork unitOfWork)
        {
            List<AccountRole> customerRoles = new List<AccountRole>
            {
                AccountRole.Customer,
                AccountRole.CustomerGold,
                AccountRole.CustomerSilver,
                AccountRole.CustomerBronze,
            };
            List<AccountRole> storeRoles= new List<AccountRole>
            {
                AccountRole.Staff,
                AccountRole.Manager,
                AccountRole.Admin,
            };
            await unitOfWork.BeginTransactionAsync();
            customerRoles.ForEach(async r => await accountRoleRepository.Create(r));
            storeRoles.ForEach(async r => await accountRoleRepository.Create(r));

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

        }
    }
}
