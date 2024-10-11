using Azure.Storage.Blobs;
using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Repositories;
using DiamondShop.Domain.Repositories.JewelryModelRepo;
using DiamondShop.Domain.Repositories.JewelryRepo;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.PromotionsRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Infrastructure.Databases;
using DiamondShop.Infrastructure.Databases.Interceptors;
using DiamondShop.Infrastructure.Databases.Repositories;
using DiamondShop.Infrastructure.Databases.Repositories.JewelryModelRepo;
using DiamondShop.Infrastructure.Databases.Repositories.JewelryRepo;
using DiamondShop.Infrastructure.Databases.Repositories.OrderRepo;
using DiamondShop.Infrastructure.Databases.Repositories.PromotionsRepo;
using DiamondShop.Infrastructure.Databases.Repositories.TransactionRepo;
using DiamondShop.Infrastructure.Identity.Models;
using DiamondShop.Infrastructure.Options;
using DiamondShop.Infrastructure.Options.Setups;
using DiamondShop.Infrastructure.Outbox;
using DiamondShop.Infrastructure.Securities;
using DiamondShop.Infrastructure.Securities.Authentication;
using DiamondShop.Infrastructure.Services;
using DiamondShop.Infrastructure.Services.Payments.Paypals;
using DiamondShop.Infrastructure.Services.Payments.Zalopays;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;

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
            services.AddPayments(configuration);

            return services;
        }
        public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDateTimeProvider, DateTimeProvider>();

            services.AddMemoryCache();

            services.AddDbContext<DiamondShopDbContext>(opt =>
            {
                opt.UseNpgsql("Host=localhost;Port=5432;Database=DiamondShopTest;Username=postgres;Password=12345;Include Error Detail=true");
                //opt.UseNpgsql(configuration.GetSection("ConnectionString:Database"));
            });
            services.AddScoped<DomainEventsPublishserInterceptors>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IAccountRoleRepository, AccountRoleRepository>();

            services.AddScoped<IDiamondRepository, DiamondRepository>();
            services.AddScoped<IDiamondShapeRepository, DiamondShapeRepository>();
            services.AddScoped<IDiamondCriteriaRepository, DiamondCriteriaRepository>();
            services.AddScoped<IDiamondPriceRepository, DiamondPriceRepository>();


            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IGiftRepository, GiftRepository>();
            services.AddScoped<IRequirementRepository, RequirementRepository>();
            services.AddScoped<IDiscountRepository, DiscountRepository>();

            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderLogRepository, OrderLogRepository>();
            services.AddScoped<IDeliveryPackageRepository, DeliveryPackageRepository>();

            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();

            services.AddScoped<IJewelryRepository, JewelryRepository>();
            services.AddScoped<IJewelrySideDiamondRepository, JewelrySideDiamondRepository>();

            services.AddScoped<IJewelryModelRepository, JewelryModelRepository>();
            services.AddScoped<IJewelryModelCategoryRepository, JewelryModelCategoryRepository>();
            services.AddScoped<IMainDiamondRepository, MainDiamondRepository>();
            services.AddScoped<ISideDiamondRepository, SideDiamondRepository>();
            services.AddScoped<ISizeRepository, SizeRepository>();
            services.AddScoped<IMetalRepository, MetalRepository>();
            services.AddScoped<ISizeMetalRepository, SizeMetalRepository>();

            services.AddSingleton<ICartService, CartService>();
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

        internal static IServiceCollection AddPayments(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddSingleton<PaypalClient>();
            services.AddSingleton<ZalopayClient>();
            services.AddTransient<IPaymentService, ZalopayPaymentService>();
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
            services.Configure<UrlOptions>(configuration.GetSection(UrlOptions.Section));
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
