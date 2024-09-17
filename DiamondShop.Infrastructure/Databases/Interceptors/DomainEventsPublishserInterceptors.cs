using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain.Common;
using DiamondShop.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiamondShop.Infrastructure.Databases.Interceptors
{
    public class DomainEventsPublishserInterceptors : SaveChangesInterceptor
    {
        private readonly IPublisher _publisher;
        private readonly IDateTimeProvider _dateTimeProvider;
        internal static JsonSerializerSettings _jsonSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All, };
        public DomainEventsPublishserInterceptors(IPublisher publisher, IDateTimeProvider dateTimeProvider)
        {
            _publisher = publisher;
            _dateTimeProvider = dateTimeProvider;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            DbContext? dbContext = eventData.Context;
            if (dbContext == null)
                return base.SavingChanges(eventData, result);
            ProcessOutboxDomainEvents(dbContext);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            DbContext? dbContext = eventData.Context;
            if (dbContext == null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            ProcessOutboxDomainEvents(dbContext);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        private void ProcessOutboxDomainEvents(DbContext dbContext)
        {
            var entitiesWithDomainEvents = dbContext.ChangeTracker
                .Entries<IAggregateRoot>()
                .Select(entry =>(IHasDomainEvent) entry.Entity)
                .ToList();
            var allDomainEvents = entitiesWithDomainEvents
                .SelectMany(entry => entry.DomainEvents)
                .Select(e => new OutboxMessages
                {
                    Id = _dateTimeProvider.UtcNow.Ticks.ToString(),
                    CreationTime = _dateTimeProvider.UtcNow,
                    Type = e.GetType().Name,
                    Content = JsonConvert.SerializeObject(e, _jsonSerializerSettings),
                })
                .ToList();
            entitiesWithDomainEvents.ForEach(ev =>
            {
                ev.ClearDomainEvents();
            });
            if(allDomainEvents.Count > 0) 
            {
                dbContext.Set<OutboxMessages>().AddRange(allDomainEvents);
            }
            //foreach (var ev in allDomainEvents)
            //{
            //    await _publisher.Publish(ev);
            //}
        }

    }
}
