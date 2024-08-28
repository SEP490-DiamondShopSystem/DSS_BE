using DiamondShop.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Repositories
{
    internal class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly DiamondShopDbContext _dbContext;
        protected readonly DbSet<T> _set;

        public BaseRepository(DiamondShopDbContext dbContext)
        {
            _dbContext = dbContext;
            _set = _dbContext.Set<T>();
        }


        public virtual async Task Create(T entity, CancellationToken token = default)
        {
             _set.AddAsync(entity, token).GetAwaiter().GetResult();
        }

        public virtual async Task Delete(T entity, CancellationToken token = default)
        {
            _set.Remove(entity);
        }

        public virtual async Task<IEnumerable<T>> GetAll(CancellationToken token = default)
        {
            return await _set.ToListAsync(token);
        }

        public virtual async Task<T?> GetById(CancellationToken token = default, params object[] ids)
        {
            return await _set.FindAsync(ids, token);
        }

        public virtual int GetCount()
        {
            return _set.Count();
        }

        public virtual IQueryable<T> GetQuery()
        {
            return _set.AsQueryable();
        }

        public virtual Task Update(T entity, CancellationToken token = default)
        {
            _set.Update(entity);
            return Task.CompletedTask;
        }
    }
}
