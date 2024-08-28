using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Infrastructure.Databases.Configurations;
using DiamondShop.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases
{
    public class DiamondShopDbContext : IdentityDbContext<
        CustomIdentityUser,
        CustomIdentityRole,
        string,
        CustomIdentityUserClaims,
        CustomIdentityUserRole,
        CustomIdentityUserLogins,
        CustomIdentityUserRoleClaim,
        CustomIdentityUserToken>
    {
        public DiamondShopDbContext(DbContextOptions options) : base(options)
        {
            
        }

        public DiamondShopDbContext()
        {
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<AccountRole> AccountRoles { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlServer("server=(local);Uid=sa;Pwd=12345;Database=BeatVisionRemake;TrustServerCertificate=true");
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=DiamondShopTest;Username=postgres;Password=12345;Include Error Detail=true");
            //optionsBuilder.AddInterceptors(_publishDomainEventsInterceptor);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            IdentityConfiguration.ApplyIdentityConfiguration(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DiamondShopDbContext).Assembly);
            
        }

    }
}
