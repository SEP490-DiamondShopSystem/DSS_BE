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
using Microsoft.AspNetCore.Authentication.Google;
using DiamondShop.Infrastructure.Options.Setups;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using DiamondShop.Infrastructure.Outbox;
using DiamondShop.Infrastructure.Databases.Interceptors;
using Quartz;
using DiamondShop.Infrastructure.Services.Payments.Paypals;

namespace DiamondShop.Infrastructure
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMyOptionsConfiguration(configuration);
            services.AddPersistance(configuration);
            services.AddMyIdentity(configuration);
            services.AddSecurity(configuration);
            services.AddBackgroundJobs(configuration);
            services.AddHttpClientAndRefit(configuration);
            services.AddPayments(configuration);
            return services;
        }
        public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();

            services.AddDbContext<DiamondShopDbContext>(opt =>
            {
                opt.UseNpgsql("Host=localhost;Port=5432;Database=DiamondShopTest;Username=postgres;Password=12345;Include Error Detail=true");
                //opt.UseNpgsql(configuration.GetSection("ConnectionString:Database"));
            });
            services.AddScoped<DomainEventsPublishserInterceptors>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountRoleRepository, AccountRoleRepository>();
            // file service persist
            services.AddSingleton((serviceProvider) =>
            {
                IOptions<ExternalUrlsOptions> getOption = serviceProvider.GetRequiredService<IOptions<ExternalUrlsOptions>>();
                if (getOption is null)
                    throw new ArgumentNullException();
                ExternalUrlsOptions option = getOption.Value;
                var newClient = new BlobServiceClient(option.Azure.ConnectionString,new BlobClientOptions() { });
                return newClient;
            });
            services.AddScoped<IBlobFileServices, AzureBlobContainerService>();
            return services;
        }
        public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer()
            .AddGoogle();
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
        public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddQuartz();
            //quartz optoin is configured inside the ConfigureSetup<>();
            //to inject the option of the job from appsettings
            services.AddQuartzHostedService(configure =>
            {
                configure.WaitForJobsToComplete = false;
            });
            return services;
        }
        internal static IServiceCollection AddHttpClientAndRefit(this IServiceCollection services, IConfiguration configuration)
        {
            
            //services.AddRefitClient<IPaypalClient>()
            //    .ConfigureHttpClient( (sp , httpClient) => {});
            return services;
        }
        internal static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddSingleton<PaypalClient>();
            return services;
        }
        public static IServiceCollection AddMyOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // this configure for current time, exist throughout the app life
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.JwtSection));
            services.Configure<GoogleAuthenticationOption>(configuration.GetSection(GoogleAuthenticationOption.Section));
            services.Configure<ExternalAuthenticationOptions>(configuration.GetSection(ExternalAuthenticationOptions.Section));
            services.Configure<AuthenticationRestrictionOption>(configuration.GetSection(AuthenticationRestrictionOption.Section));
            services.Configure<ExternalUrlsOptions>(configuration.GetSection(ExternalUrlsOptions.Section));
            services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.Section));
            services.Configure<VnpayOption>(configuration.GetSection(VnpayOption.Section));
            services.Configure<PaypalOption>(configuration.GetSection(PaypalOption.Section));
            // this also exist throughout the app life, but it is configured at the end of dependency injection,
            // allow it to inject other or override settings , also more cleaner moduler code
            services.ConfigureOptions<JwtBearerOptionSetup>();
            services.ConfigureOptions<GoogleOptionSetup>();
            services.ConfigureOptions<QuartzOptionSetup>();
            return services;
        }
        // test if origin change work

    }
}
