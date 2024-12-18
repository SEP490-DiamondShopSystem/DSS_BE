﻿using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Domain;
using DiamondShop.Infrastructure.Databases;
using DiamondShop.Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using System.Collections.Concurrent;
using System.Reflection;

namespace DiamondShop.Infrastructure.BackgroundJobs
{
    //[DisallowConcurrentExecution]
    internal class ProcessOutboxMessagesWorker : IJob
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<OutboxOptions> _outboxOptions;
        private readonly ILogger<ProcessOutboxMessagesWorker> _logger;
        private readonly DiamondShopDbContext _context;
        private readonly IPublisher _publisher;

        public ProcessOutboxMessagesWorker(IDateTimeProvider dateTimeProvider, IUnitOfWork unitOfWork, IOptions<OutboxOptions> outboxOptions, ILogger<ProcessOutboxMessagesWorker> logger, DiamondShopDbContext context, IPublisher publisher)
        {
            _dateTimeProvider = dateTimeProvider;
            _unitOfWork = unitOfWork;
            _outboxOptions = outboxOptions;
            _logger = logger;
            _context = context;
            _publisher = publisher;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("outbox processor is called");
            await RemoveProcessedMessage(context);
            await ProcessingMessage(context);
        }
        private async Task RemoveProcessedMessage(IJobExecutionContext jobExecutionContext)
        {
            var totalDeletedMessage =  _context.OutboxMessages.Where(m => m.CompleteTime != null && m.Exception == null).ExecuteDelete();
            if(totalDeletedMessage > 0)
                _logger.LogInformation("Deleted {0} message",totalDeletedMessage);
        }
        private async Task ProcessingMessage(IJobExecutionContext jobExecutionContext)
        {
            await _unitOfWork.BeginTransactionAsync();
            var getUnprocessMessage = await _context.OutboxMessages
                .FromSqlRaw("SELECT * " + //Id, Type, Content, ProcessTime, CreationTime , CompleteTime, Exception
                            "FROM outbox_message om " +
                            "WHERE om.\"CompleteTime\" IS NULL " +
                            "ORDER BY om.\"CreationTime\" " +
                            "LIMIT {0} FOR UPDATE SKIP LOCKED", _outboxOptions.Value.BatchSize)
                .ToListAsync();
            var concurrentMessage = new ConcurrentBag<OutboxMessages>(getUnprocessMessage) ;
            foreach (var message in getUnprocessMessage)
            {
                Exception any = null;
                try
                {
                    Assembly asm = typeof(DomainLayer).Assembly;
                    var messageType = System.Type.GetType(message.Type); //asm.GetType(message.Type);
                    var parsedMessage = JsonConvert.DeserializeObject(message.Content, messageType);
                    await _publisher.Publish(parsedMessage);
                    message.Exception = null;
                    message.CompleteTime = _dateTimeProvider.UtcNow;
                    _context.OutboxMessages.Update(message);

                }
                catch (Exception ex)
                {
                    any = ex;
                    _logger.LogError(ex,"exception while processing outbox id: {0}",message.Id);
                    message.Exception = ex.Message;
                    message.ProcessTime += 1;
                }
                message.Exception = any?.ToString();
                _context.OutboxMessages.Update(message);
                
            }
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            if(getUnprocessMessage.Count > 0)
                _logger.LogInformation("Processed message completed, total {0}", getUnprocessMessage.Count);
            else
                _logger.LogInformation("No message to process");
        }
    }
}
