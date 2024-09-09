using DiamondShop.Domain.Models.CustomerAggregate;
using DiamondShop.Domain.Models.RoleAggregate;
using DiamondShop.Domain.Models.StaffAggregate;
using DiamondShop.Domain.Roles;
using DiamondShop.Infrastructure.Databases.Configurations;
using DiamondShop.Infrastructure.Databases.Interceptors;
using DiamondShop.Infrastructure.Identity.Models;
using DiamondShop.Infrastructure.Outbox;
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
        private readonly DomainEventsPublishserInterceptors _domainEventsInterceptor;
        public DiamondShopDbContext(DbContextOptions options, DomainEventsPublishserInterceptors interceptors) : base(options)
        {
            _domainEventsInterceptor = interceptors;
        }

        public DiamondShopDbContext()
        {
        }
        // Outbox Related //
        internal DbSet<OutboxMessages> OutboxMessages { get; set; }
        //Roles Related//
        public DbSet<AccountRole> AccountRoles { get; set; }

        //Roles Related//

        //Application//
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staff { get; set; }
        //Application//

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //optionsBuilder.UseSqlServer("server=(local);Uid=sa;Pwd=12345;Database=BeatVisionRemake;TrustServerCertificate=true");
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=DiamondShopTest;Username=postgres;Password=12345;Include Error Detail=true");
            //configure interceptors
            optionsBuilder.AddInterceptors(_domainEventsInterceptor);
            //configure logging
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
