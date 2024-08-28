using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Repositories
{
    internal class StaffRepository : BaseRepository<Staff>, IStaffRepository
    {
        public StaffRepository(DiamondShopDbContext dbContext) : base(dbContext)
        {
        }
        public override async Task Update(Staff entity, CancellationToken token = default)
        {
           
            var userRoles = entity.Roles;
            userRoles.ForEach(role => _dbContext.Entry(role).State = EntityState.Unchanged);
            base.Update(entity, token);
        }
    }
    
}
