using DiamondShop.Application.Services.Data;
using DiamondShop.Domain.Repositories;
using DiamondShop.Infrastructure.Databases;
using DiamondShop.Infrastructure.Securities;
using System.Net.NetworkInformation;

namespace DiamondShop.Api.Extensions
{
    internal static class SeedDataExtensions
    {
        public static async void SeedData(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            IAccountRoleRepository roleRepository = scope.ServiceProvider.GetRequiredService<IAccountRoleRepository>();
            IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await Seeding.SeedAsync(roleRepository, unitOfWork);
        }
    }
}
