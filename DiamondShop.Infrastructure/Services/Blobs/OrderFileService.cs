﻿using Azure.Storage.Blobs;
using DiamondShop.Application.Commons.Models;
using DiamondShop.Application.Commons.Utilities;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Interfaces.Orders;
using DiamondShop.Domain.Common.ValueObjects;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Models.Orders.Entities;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Infrastructure.Options;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading;

namespace DiamondShop.Infrastructure.Services.Blobs
{
    internal class OrderFileService : AzureBlobContainerService, IOrderFileServices
    {
        internal const string PARENT_FOLDER = "Order";
        internal const string IMAGES_FOLDER = "Images";
        internal const string TRANSACTION_FOLDER = IMAGES_FOLDER + "/" + "Transaction";
        internal const string ORDERLOG_FOLDER = IMAGES_FOLDER + "/" + "OrderLog";
        internal const string DELIMITER = "/";
        internal const string INVOICE_FOLDER = "Invoices";
        internal const string CONFIRM_DELIVERY_FOLDER_VIDEO = CONFIRM_DELIVERY_FOLDER+ "/"+  "Videos";
        internal const string CONFIRM_DELIVERY_FOLDER = "ConfirmDelivery";
        internal const string CONFIRM_DELIVERY_FOLDER_IMAGE = CONFIRM_DELIVERY_FOLDER + "/" + "Images";
        private OrderGalleryTemplate _cachedGalleryTemplate;
        private readonly IPdfService _pdfService;
        private readonly IOptions<ExternalUrlsOptions> _externalUrlsOptions;
        private readonly IOptions<PublicBlobOptions> _publicBlobOptions;
        public OrderFileService(BlobServiceClient blobServiceClient, ILogger<AzureBlobContainerService> logger, IOptions<ExternalUrlsOptions> externalUrlsOptions,IOptions<PublicBlobOptions> publicBlobOptions, IPdfService pdfService) : base(blobServiceClient, logger, externalUrlsOptions)
        {
            _externalUrlsOptions = externalUrlsOptions;
            _publicBlobOptions = publicBlobOptions;
            _pdfService = pdfService;
        }

        public async Task<OrderGalleryTemplate> GetAllOrderImages(Order order, CancellationToken cancellationToken = default)
        {
            var paths = GetAzureFilePath(order);
            var getFolders = await GetFolders(paths, cancellationToken);
            return MapPathsToCorrectGallery(order, getFolders, cancellationToken);
        }

        public async Task<Result<List<Media>>> GetOrderLogImages(Order order, OrderLog orderLog, CancellationToken cancellationToken = default)
        {
            var gallery = GetCachedGalleryTemplate(order, cancellationToken);
            var key = $"{ORDERLOG_FOLDER}/{GetOrderLogNameIdentifier(orderLog)}";
            var isSuccess = gallery.OrderLogImages.TryGetValue(key, out var media);
            if (isSuccess== false)
                return new List<Media>();
            return media;
        }

        public async Task<Result<List<Media>>> GetOrderTransactionImages(Order order, Transaction transaction, CancellationToken cancellationToken = default)
        {
            var gallery = GetCachedGalleryTemplate(order, cancellationToken);
            var key = $"{TRANSACTION_FOLDER}/{GetTransactionNameIdentifier(transaction)}";
            var getMedia = gallery.OrderTransactionImages[key];
            if (getMedia == null)
                return new List<Media>();
            return getMedia;
        }

        public async Task<Result<string[]>> UploadOrderLogImage(Order order, OrderLog log, FileData[] images, CancellationToken cancellationToken = default)
        {
            string basePath = GetAzureFilePath(order);
            basePath = $"{basePath}/{ORDERLOG_FOLDER}";
            List<Task<Result<string[]>>> uploadTasks = new();
            foreach (var file in images)
            {
                var finalPath = $"{basePath}/{GetOrderLogNameIdentifier(log)}";
                uploadTasks.Add(UploadFromBasePath(finalPath, new List<FileData> { file }.ToArray(), cancellationToken));
            }
            var results = await Task.WhenAll(uploadTasks);
            var stringResult = results.Where(r => r.IsSuccess).SelectMany(r => r.Value).ToArray();
            if (stringResult.Length == 0)
                return Result.Fail(FileUltilities.Errors.UploadFail);
            return Result.Ok(stringResult);
        }

        public async Task<Result<string[]>> UploadOrderTransactionImage(Order order, Transaction transaction, FileData[] images, CancellationToken cancellationToken = default)
        {
            string basePath = GetAzureFilePath(order);
            basePath = $"{basePath}/{TRANSACTION_FOLDER}";
            List<Task<Result<string[]>>> uploadTasks = new();
            foreach (var file in images)
            {
                var finalPath = $"{basePath}/{GetTransactionNameIdentifier(transaction)}";
                uploadTasks.Add(UploadFromBasePath(finalPath, new List<FileData> { file }.ToArray(), cancellationToken));
            }
            var results = await Task.WhenAll(uploadTasks);
            var stringResult = results.Where(r => r.IsSuccess).SelectMany(r => r.Value).ToArray();
            if (stringResult.Length == 0)
                return Result.Fail(FileUltilities.Errors.UploadFail);
            return Result.Ok(stringResult);
        }
        public OrderGalleryTemplate MapPathsToCorrectGallery(Order order, List<Media> paths, CancellationToken cancellationToken = default)
        {
            var basePath = GetAzureFilePath(order);
            var gallery = new OrderGalleryTemplate();
            //Action<string, Media> AddToCategory = (string tobeComparedPath, Media originalMedia) =>
            //{
            //    int lastSlashIndex = tobeComparedPath.LastIndexOf('/');
            //    if (lastSlashIndex >= 0)
            //    {
            //        var key = tobeComparedPath.Substring(0, lastSlashIndex);
            //        if (!gallery.Gallery.ContainsKey(key))
            //            gallery.Gallery[key] = new List<Media>();
            //        gallery.Gallery[key].Add(originalMedia);
            //    }
            //};
            foreach (var path in paths)
            {
                var tobeComparedPath = path.MediaPath.Replace(basePath + DELIMITER, string.Empty);
                if (tobeComparedPath.StartsWith(TRANSACTION_FOLDER))
                {
                    gallery.AddOrderTransactionImages(tobeComparedPath, path);
                }

                else if (tobeComparedPath.StartsWith(ORDERLOG_FOLDER))
                {
                    gallery.AddOrderLogImages(tobeComparedPath, path);
                }else if (tobeComparedPath.StartsWith(CONFIRM_DELIVERY_FOLDER))
                {
                    if(tobeComparedPath.StartsWith(CONFIRM_DELIVERY_FOLDER_IMAGE))
                        gallery.AddConfirmOrderImages(new List<Media> { path });
                    if(tobeComparedPath.StartsWith(CONFIRM_DELIVERY_FOLDER_VIDEO))
                        gallery.AddConfirmOrderVideo(path);
                }
                //AddToCategory(tobeComparedPath, path);
                else { }
                    //gallery.BaseImages.Add(path);
                
            }
            return gallery;
        }
        private string GetOrderLogNameIdentifier(OrderLog log)
        {
            var logTypeIdentifier = log.Status.ToString();
            var isParent = log.PreviousLogId is null;
            var parentIdentifider = isParent ? "Parent" : "Child";
            return $"{logTypeIdentifier}_{parentIdentifider}_{log.Id.Value}";
        }
        private string GetTransactionNameIdentifier(Transaction transaction)
        {
            return $"{transaction.Id.Value}";
        }
        private OrderGalleryTemplate GetCachedGalleryTemplate(Order order, CancellationToken cancellationToken = default)
        {
            if (_cachedGalleryTemplate is null)
            {
                _cachedGalleryTemplate = GetAllOrderImages(order, cancellationToken).Result;
            }
            return _cachedGalleryTemplate;
        }

        private string GetAzureFilePath(Order order)
        {
            return $"{PARENT_FOLDER}/{order.Id.Value}";
        }

        public async Task<Result<Media>> CreateOrderInvoice(Order fullDetailOrder, CancellationToken cancellationToken = default)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(fullDetailOrder.Account);
                ArgumentNullException.ThrowIfNull(fullDetailOrder.Items);
                ArgumentNullException.ThrowIfNull(fullDetailOrder.Transactions);
            }
            catch 
            {
                return Result.Fail("Invoice cần đầy đủ thông tin");
            }
            string htmlString = _pdfService.GetTemplateHtmlStringFromOrder(fullDetailOrder,fullDetailOrder.Account);
            var uploadStream = _pdfService.ParseHtmlToPdf(htmlString);
            string basePath = GetAzureFilePath(fullDetailOrder);
            basePath = $"{basePath}/{INVOICE_FOLDER}";
            string fileName = fullDetailOrder.OrderCode;
            var uploadResult = await UploadFromBasePath(basePath, new FileData[] { new FileData(fileName, "pdf", "application/pdf", uploadStream) }, cancellationToken);
            return new Media
            {
                ContentType = "application/pdf",
                MediaName = fileName,
                MediaPath = ToRelativePath(uploadResult.Value.First()),
            };
        }

        public async Task<Result<Media>> GetOrCreateOrderInvoice(Order fullDetailOrder, CancellationToken cancellationToken = default)
        {
            var paths = GetAzureFilePath(fullDetailOrder);
            var getFolders = await GetFolders(paths, cancellationToken);
            var basePath = $"{paths}/{INVOICE_FOLDER}"+ "/";
            Media foundedMedia = null;
             foreach (var path in getFolders)
            {
                if (path.MediaPath.StartsWith(basePath))
                {
                    foundedMedia =  new Media
                    {
                        ContentType = path.ContentType,
                        MediaName = path.MediaName,
                        MediaPath = path.MediaPath,
                    };
                    break;
                }
            }
            if(foundedMedia == null)
            {
                return await CreateOrderInvoice(fullDetailOrder, cancellationToken);
            }
            return foundedMedia;
            //throw new NotImplementedException();
        }

        public async Task<Result<List<Media>>> SaveOrderConfirmDeliveryImage(Order finishDeliveryOrder, List<FileData> images)
        {
            string basePath = GetAzureFilePath(finishDeliveryOrder);
            basePath = $"{basePath}/{CONFIRM_DELIVERY_FOLDER_IMAGE}";
            List<Task<Result<string[]>>> uploadTasks = new();
            foreach (var file in images)
            {
                var finalPath = $"{basePath}/{file.FileName}.{file.FileExtension}";
                uploadTasks.Add(UploadFromBasePath(finalPath, new List<FileData> { file }.ToArray()));
            }
            var results = await Task.WhenAll(uploadTasks);
            var stringResult = results.Where(r => r.IsSuccess).SelectMany(r => r.Value).ToArray();
            if (stringResult.Length == 0)
                return Result.Fail(FileUltilities.Errors.UploadFail);
            var getFolders = await GetFolders(basePath);
            return Result.Ok(getFolders);
        }

        public async Task<Result<Media>> SaveOrderConfirmVideo(Order fnishDeliveryOrder, FileData video)
        {
            if (FileUltilities.IsVideoFileContentType(video.contentType) == false)
                return Result.Fail("Không phải file video");
            string basePath = GetAzureFilePath(fnishDeliveryOrder);
            basePath = $"{basePath}/{CONFIRM_DELIVERY_FOLDER_VIDEO}";
            if (IsAnyFileExist(basePath).Result)
            {
                await DeleteFileAsync(basePath);
            }
            var result = await UploadFromBasePath(basePath, new List<FileData> { video }.ToArray());
            if (result.IsFailed)
                return Result.Fail(result.Errors);
            var media = new Media
            {
                ContentType = video.contentType,
                MediaName = video.FileName,
                MediaPath = ToRelativePath(result.Value.First()),
            };
            return media;
        }
    }
}
