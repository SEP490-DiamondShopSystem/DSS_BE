using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Common;
using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.CustomerAggregate.ValueObjects;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Roles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Repositories
{
    internal class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {

        public CustomerRepository(DiamondShopDbContext dbContext) : base(dbContext)
        {
            
        }
        public override async Task Create(Customer entity, CancellationToken token = default)
        {
            var userRoles = entity.Roles;
            base.Create(entity, token);
            foreach (var role in userRoles)
            {
                var changetracker  = _dbContext.ChangeTracker.Entries();
                _dbContext.Entry(role).State = EntityState.Unchanged;
            }
            
        }
        public override async Task Update(Customer entity, CancellationToken token = default)
        {
            var userRoles  = entity.Roles;
            base.Update(entity, token);
            foreach (var role in userRoles)
            {
                var changetracker = _dbContext.ChangeTracker.Entries();
                _dbContext.Entry(role).State = EntityState.Unchanged;
            }
            //var changeEntity = _dbContext.ChangeTracker.Entries().ToList();
            
        }
        public override async Task<Customer?> GetById(CancellationToken token = default, params object[] ids)
        {
            CustomerId customerId = (CustomerId)ids[0];
            var find = await _set.Include(c => c.Roles).FirstOrDefaultAsync(c => c.Id == customerId);
            return find;
        }

        public async Task<Customer?> GetByIdentityId(string identityId, CancellationToken cancellationToken = default)
        {
            return await _set.Include(c => c.Roles).FirstOrDefaultAsync(a => a.IdentityId == identityId, cancellationToken);
        }
    }
}
