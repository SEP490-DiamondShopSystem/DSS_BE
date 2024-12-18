﻿using DiamondShop.Infrastructure.BackgroundJobs;
using DiamondShop.Infrastructure.Outbox;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Options.Setups
{
    internal class QuartzOptionSetup : IConfigureNamedOptions<QuartzOptions>
    {
        private readonly IOptions<OutboxOptions> _outboxOption;

        public QuartzOptionSetup(IOptions<OutboxOptions> outboxOption)
        {
            _outboxOption = outboxOption;
        }

        public void Configure(string? name, QuartzOptions options)
        {
            Configure(options);
        }

        public void Configure(QuartzOptions options)
        {
            string outboxJobName = nameof(ProcessOutboxMessagesWorker);
            options.AddJob<ProcessOutboxMessagesWorker>(config => config.WithIdentity(outboxJobName))
                .AddTrigger(config => config.ForJob(outboxJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInSeconds(_outboxOption.Value.IntervalSeconds * 3)
                        .RepeatForever()));

            string promotionJobName = nameof(PromotionManagerWorker);
            options.AddJob<PromotionManagerWorker>(config => config.WithIdentity(promotionJobName))
                .AddTrigger(config => config.ForJob(promotionJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInMinutes(10)
                        .RepeatForever()));

            string discountJobName = nameof(DiscountManagerWorker);
            options.AddJob<DiscountManagerWorker>(config => config.WithIdentity(discountJobName))
                .AddTrigger(config => config.ForJob(discountJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInMinutes(10)
                        .RepeatForever()));

            string orderJobName = nameof(OrderManagementWorker);
            options.AddJob<OrderManagementWorker>(config => config.WithIdentity(orderJobName))
                .AddTrigger(config => config.ForJob(orderJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInMinutes(40)
                        .RepeatForever()));

            string accountJobName = nameof(AccountManagerWorkers);
            options.AddJob<AccountManagerWorkers>(config => config.WithIdentity(accountJobName))
                .AddTrigger(config => config.ForJob(accountJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInMinutes(5)
                        .RepeatForever()));

            string customizeRequestJobName = nameof (CustomizeRequestWorker);
            options.AddJob<CustomizeRequestWorker>(config => config.WithIdentity(customizeRequestJobName))
                .AddTrigger(config => config.ForJob(customizeRequestJobName)
                .WithSimpleSchedule(schedule => schedule.WithIntervalInMinutes(40).RepeatForever()));

            string productLockExpiredJobName = nameof(ProductLockExpiredWorkers);
            options.AddJob<ProductLockExpiredWorkers>(config => config.WithIdentity(productLockExpiredJobName))
                .AddTrigger(config => config.ForJob(productLockExpiredJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInMinutes(5)
                            .RepeatForever()));

            string notificationExpiredJobName = nameof(NotificationWorkers);
            options.AddJob<NotificationWorkers>(config => config.WithIdentity(notificationExpiredJobName))
                .AddTrigger(config => config.ForJob(notificationExpiredJobName)
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInMinutes(5)
                            .RepeatForever()));
        }
    }
}
