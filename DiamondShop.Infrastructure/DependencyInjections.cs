using BeatvisionRemake.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DiamondShop.Infrastructure.Options;
using System.ComponentModel;
using DiamondShop.Infrastructure.Databases;
using Microsoft.EntityFrameworkCore;
using DiamondShop.Domain.Repositories;
using DiamondShop.Infrastructure.Databases.Repositories;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Infrastructure.Services;
using BeatvisionRemake.Application.Services.Interfaces;
using DiamondShop.Application.Services.Data;
using DiamondShop.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using DiamondShop.Infrastructure.Securities;
using DiamondShop.Infrastructure.Securities.Authentication;

namespace DiamondShop.Infrastructure
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();

            services.AddPersistance(configuration); 
            services.AddSecurity(configuration);
            services.AddMyIdentity(configuration);
            return services;
        }
        public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DiamondShopDbContext>( opt =>
            {
                opt.UseNpgsql("Host=localhost;Port=5432;Database=DiamondShopTest;Username=postgres;Password=12345;Include Error Detail=true");
                //opt.UseNpgsql(configuration.GetSection("ConnectionString:Database"));
            });
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();    
            services.AddScoped<IAccountRoleRepository, AccountRoleRepository>();
            
            return services;
        }
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.JwtSection));
            var authOpt = new JwtOptions();
            configuration.GetSection(JwtOptions.JwtSection).Bind(authOpt);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer();
            services.Configure<JwtBearerOptions>(config =>
            {
                var validationParam = config.TokenValidationParameters;
                validationParam.ValidIssuer = authOpt.ValidIssuer;
                validationParam.ValidAudience = authOpt.Audience;
                validationParam.ValidateIssuer = false;
                validationParam.ValidateAudience = false;
                validationParam.ValidateLifetime = true;
                validationParam.ValidateIssuerSigningKey = true;
                validationParam.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOpt.SigningKey));
                config.RequireHttpsMetadata = authOpt.RequireHttpsMetadata;
                config.MetadataAddress = authOpt.MetadataUrl;
            });
            services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();


            return services;
        }
        public static IServiceCollection AddMyIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<CustomIdentityUser, CustomIdentityRole>()
               .AddEntityFrameworkStores<DiamondShopDbContext>()
               .AddRoleManager<CustomRoleManager>()
               .AddSignInManager<CustomSigninManager>()
               .AddUserManager<CustomUserManager>()
               .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                var userOpt = options.User;
                var passOpt = options.Password;
                var tokenOpt = options.Tokens;
                userOpt.RequireUniqueEmail = true;

                passOpt.RequireNonAlphanumeric = false;
                passOpt.RequireDigit = true;
                passOpt.RequireLowercase = false;
                passOpt.RequireUppercase = false;
                passOpt.RequiredLength = 3;
            });
            return services;
        }
    }
}
