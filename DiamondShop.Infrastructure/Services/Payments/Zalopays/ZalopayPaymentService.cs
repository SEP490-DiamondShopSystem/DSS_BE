using DiamondShop.Application.Services.Data;
using DiamondShop.Application.Services.Interfaces;
using DiamondShop.Application.Services.Models;
using DiamondShop.Domain.Models.Orders;
using DiamondShop.Domain.Repositories.OrderRepo;
using DiamondShop.Domain.Repositories.TransactionRepo;
using DiamondShop.Infrastructure.Services.Payments.Zalopays.Models.Responses;
using DiamondShop.Infrastructure.Services.Payments.Zalopays.Models;
using DiamondShop.Infrastructure.Services.Payments.Zalopays.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DiamondShop.Domain.Models.Transactions;
using DiamondShop.Domain.Services.interfaces;
using FluentResults;
using DiamondShop.Domain.Models.Transactions.Entities;
using DiamondShop.Domain.Models.Orders.ValueObjects;
using DiamondShop.Infrastructure.Services.Payments.Zalopays.Constants;
using DiamondShop.Domain.Models.Transactions.Enum;
using DiamondShop.Domain.BusinessRules;
using OpenQA.Selenium.DevTools.V127.Page;
using Microsoft.Extensions.Options;
using DiamondShop.Infrastructure.Options;

namespace DiamondShop.Infrastructure.Services.Payments.Zalopays
{
    internal class ZalopayPaymentService : IPaymentService
    {
        private readonly ILogger<ZalopayPaymentService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderTransactionService _orderTransactionService;
        private readonly IOptions<UrlOptions> _urlOptions;
        private static string appid = "2554";
        private static string key1 = "sdngKKJmqEMzvh5QQcdD2A9XBSKUNaYn";
        private static string key2 = "trMrHtvjo6myautxDUiAcYsVtaeQ8nhf";
        private static string ReturnUri = "api/Zalopay/Return";
        private static string CallbackUri = "api/Zalopay/Callback";


        private static string getBankListUrl = "https://sbgateway.zalopay.vn/api/getlistmerchantbanks";
        private static string create_order_url = "https://sb-openapi.zalopay.vn/v2/create";
        private static string query_order_url = "https://sb-openapi.zalopay.vn/v2/query";
        private static string refund_url = "https://sb-openapi.zalopay.vn/v2/refund";
        private static string query_refund_url = "https://sb-openapi.zalopay.vn/v2/query_refund";

        public ZalopayPaymentService(ILogger<ZalopayPaymentService> logger, IUnitOfWork unitOfWork, IOrderRepository orderRepository, ITransactionRepository transactionRepository, IPaymentMethodRepository paymentMethodRepository, IHttpContextAccessor httpContextAccessor, IOrderTransactionService orderTransactionService, IOptions<UrlOptions> urlOptions)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _transactionRepository = transactionRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _httpContextAccessor = httpContextAccessor;
            _orderTransactionService = orderTransactionService;
            _urlOptions = urlOptions;
        }

        public async Task<object> Callback()
        {
            var httpClient = _httpContextAccessor.HttpContext;
            if (httpClient == null)
                throw new Exception("cannot found httpClient");
            var result = new Dictionary<string, object>();
            var cbData = await httpClient.Request.ReadFromJsonAsync<ZalopayCallbackRequest>();
            try
            {
                var dataStr = Convert.ToString(cbData.data);
                var reqMac = Convert.ToString(cbData.mac);
                var mac = HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, key2, dataStr);
                // kiểm tra callback hợp lệ (đến từ ZaloPay server)
                if (!reqMac.Equals(mac))
                {
                    // callback không hợp lệ
                    result["return_code"] = -1;
                    result["return_message"] = "mac not equal";
                }
                else
                {
                    // thanh toán thành công
                    // merchant cập nhật trạng thái cho đơn hàng
                    var dataObject = JsonConvert.DeserializeObject<ZalopayPaymentDataField>(dataStr);
                    var embedDataObject = JsonConvert.DeserializeObject<ZalopayEmbeddedData>(dataObject.embed_data_string);
                    var itemObjectList = JsonConvert.DeserializeObject<List<ZalopayItem>>(dataObject.item_string);
                    PaymentMetadataBodyPerTransaction metaData = JsonConvert.DeserializeObject<PaymentMetadataBodyPerTransaction>(embedDataObject.columninfo);
                    dataObject.ZalopayEmbeddedData = embedDataObject;
                    dataObject.Items = itemObjectList;
                    var appGivenId = dataObject.app_trans_id;
                    var paymentGateId = dataObject.zp_trans_id.ToString();
                    ArgumentNullException.ThrowIfNull(appGivenId);
                    ArgumentNullException.ThrowIfNull(paymentGateId);
                    // ta thu lay transaction, neu da ton tai thi check status roi tinh tiep
                    // neu chua ton tai ==> auto save transaction
                    Transaction? tryGetTransaction = await _transactionRepository.GetByAppAndPaygateId(appGivenId,paymentGateId);
                    List<PaymentMethod> paymentMethods = await _paymentMethodRepository.GetAll();
                    var zalopayMethod = paymentMethods.First(x => x.MethodName.ToUpper() == "ZALOPAY");
                    _logger.LogInformation("update order's status = success where app_trans_id = {0}", dataObject.app_trans_id);
                    if (tryGetTransaction == null) // check neu thanh cong, check DB xem transaction ton tai hay chuaw thi tra ve return_code = 1
                    {
                        var orderIdParsed = OrderId.Parse(metaData.ForOrderId);
                        var newTran = Transaction.CreatePayment(zalopayMethod.Id, orderIdParsed, metaData.Description,dataObject.app_trans_id,dataObject.zp_trans_id.ToString(),metaData.TimeStampe,dataObject.amount,DateTime.UtcNow);
                        await _transactionRepository.Create(newTran);
                        result["return_code"] = 1;
                        result["return_message"] = "success";
                    }
                    else// neu ton tai Transaction, laf transaction da callback roi => tra ve = 2, la giao dich da xu ly
                    {
                        result["return_code"] = 2;
                        result["return_message"] = "success";
                    } 
                    //if () { } // con lai thi loi, ko callback
                }
            }
            catch (Exception ex)
            {
                result["return_code"] = 0; // ZaloPay server sẽ callback lại (tối đa 3 lần)
                result["return_message"] = ex.Message;
            }
            // thông báo kết quả cho ZaloPay server
            return result;
        }

        public async Task<Result<PaymentLinkResponse>> CreatePaymentLink(PaymentLinkRequest paymentLinkRequest, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(paymentLinkRequest.Amount);
            var callbackUrl = string.Concat(_urlOptions.Value.HttpsUrl, CallbackUri);
            var returnUrl = string.Concat(_urlOptions.Value.HttpsUrl, ReturnUri);

            var embed_data = new ZalopayEmbeddedData
            {
                columninfo = "",
                redirecturl = returnUrl,
                preferred_payment_method = ["domestic_card", "account"],
            };
            var param = new Dictionary<string, string>();
            var app_trans_id = DateTime.UtcNow.ToString("yyyyMMddHHmmss"); //rnd.Next(100000000); // Generate a random order's ID.
            var userId = paymentLinkRequest.Account.Id.Value;
            var amount = paymentLinkRequest.Amount;
            var timeStampe = ZalopayUtils.GetTimeStamp().ToString();
            var description = paymentLinkRequest.Description is null ? $"thanh toan cho don hang {paymentLinkRequest.Order.Id.Value}, timestampe = {timeStampe}" : paymentLinkRequest.Description;
            PaymentMetadataBodyPerTransaction descriptionBodyJson = new PaymentMetadataBodyPerTransaction
            {
                GeneratedCode = app_trans_id,
                TimeStampe = timeStampe,
                ForAccountId = paymentLinkRequest.Account.Id.Value,
                ForOrderId = paymentLinkRequest.Order.Id.Value,
                Description = description,
            };
            //insert meta data
            embed_data.columninfo = JsonConvert.SerializeObject(descriptionBodyJson);

            param.Add("app_id", appid);
            param.Add("app_user", userId);
            param.Add("app_time", timeStampe);
            param.Add("amount", amount.ToString());
            param.Add("app_trans_id", DateTime.Now.ToString("yyMMdd") + "_" + app_trans_id); // mã giao dich có định dạng yyMMdd_xxxx
            param.Add("embed_data", JsonConvert.SerializeObject(embed_data));
            //param.Add("item", JsonConvert.SerializeObject(body.item));
            param.Add("description", description);
            param.Add("bank_code", "");
            param.Add("callback_url", callbackUrl);
            var data = appid + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"] + "|"
                + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, key1, data));
            var result = await HttpHelper.PostFormAsync<ZalopayCreateOrderResponse>(create_order_url, param);
            if (result.return_code != 1)
                return Result.Fail($"fail with message from paygate: {result.sub_return_message} and code: {result.return_code} ");
            return Result.Ok(new PaymentLinkResponse { PaymentUrl = result.order_url ,QrCode = result.qr_code });
        }

        public async Task<PaymentRefundDetail> GetRefundDetail(Transaction refundTransactionType)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("app_id", appid);
            param.Add("timestamp", refundTransactionType.TimeStampe);
            param.Add("m_refund_id", refundTransactionType.AppTransactionCode);//"190308_2553_xxxxxx");

            var data = appid + "|" + param["m_refund_id"] + "|" + param["timestamp"];
            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, key1, data));

            var result = await HttpHelper.PostFormAsync<ZalopayRefundResponse>(query_refund_url, param);
            var code = result.return_code.ToString();
            bool isProcessing = code switch
            {
                ZalopayReturnCode.PROCESSING => true,
                _ => false
            } ;
            return new PaymentRefundDetail()
            {
                ReturnCode = result.return_code,
                SubReturnMessage = result.sub_return_message,
                SubReturnCode = result.sub_return_code,
                ReturnMessage = result.return_message,
                IsProcessing = isProcessing,
                FineAmount = refundTransactionType.FineAmount,
                RefundAmount = refundTransactionType.TransactionAmount,
                PaygateTransactionId = refundTransactionType.PaygateTransactionCode
            };
        }

        public async Task<Result<PaymentRefundDetail>> Refund(Order order,Transaction forTransaction, decimal fineAmount, string description = null)
        {
            var getTransactions = await _transactionRepository.GetByOrderId(order.Id);
            if( (order.TotalRefund + order.TotalFine) >= order.TotalPrice)
            {
                return Result.Fail("order is fully refunded");
            }
            var totalPaidTransaction = getTransactions.Where(t => t.TransactionType == TransactionType.Pay).Sum(t => t.TransactionAmount);
            var totalRefundTransaction = getTransactions.Where(t => t.TransactionType == TransactionType.Refund).Sum(t => t.TotalAmount );
            
            var timestamp = ZalopayUtils.GetTimeStamp().ToString();
            var rand = new Random();
            var uid = timestamp + "" + rand.Next(111, 999).ToString();
            var appMerchantGenId = DateTime.UtcNow.ToString("yyMMdd") + "_" + appid + "_" + uid;
            var fineAmountRounded = MoneyVndRoundUpRules.RoundAmountFromDecimal(fineAmount);
            //var refundAmount = (long) forTransaction.TotalAmount - fineAmountRounded; 
            //if (refundAmount <= 0)
            //    throw new Exception("invalid refund amount");

            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("app_id", appid);
            param.Add("m_refund_id", appMerchantGenId);
            param.Add("zp_trans_id", forTransaction.PaygateTransactionCode);
            param.Add("amount", ((long)forTransaction.TotalAmount).ToString());
            param.Add("timestamp", timestamp);
            param.Add("description", description is null ? $"hoan tra don hang cho giao dich: {forTransaction.AppTransactionCode} cua don hang: {order.Id.Value} " : description );
            string data = null;
            if (fineAmountRounded > 0)
            {
                if (fineAmountRounded < forTransaction.TotalAmount)
                {
                    param.Add("refund_fee_amount", fineAmountRounded.ToString());
                    data = appid + "|" + param["zp_trans_id"] + "|" + param["amount"] + "|" + param["refund_fee_amount"] + "|" + param["description"] + "|" + param["timestamp"];
                }
                else
                    throw new Exception("refund fee is greater than the total amount of refund");
            }
            else
                data = appid + "|" + param["zp_trans_id"] + "|" + param["amount"] + "|" + param["description"] + "|" + param["timestamp"];

            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, key1, data));

            var result = await HttpHelper.PostFormAsync<ZalopayRefundResponse>(refund_url, param);
            if(result.return_code.ToString() == ZalopayReturnCode.FAIL) 
            {
                return Result.Fail($"fail with message from paygate: {result.sub_return_message} and code: {result.return_code} ");
            }
            PaymentRefundDetail paymentRefundDetail = null;
            var transaction = Transaction.CreateRefund(forTransaction.PayMethodId, order.Id, forTransaction.Id, description, appMerchantGenId, result.refund_id.ToString(), timestamp, forTransaction.TotalAmount, fineAmountRounded);
            if (result.return_code.ToString() == ZalopayReturnCode.PROCESSING)
            {
                _logger.Log(LogLevel.Information, "refund is processing, need to check later");
                // try to get 3 times 
                for(int i = 0; i < 5; i++)
                {
                    paymentRefundDetail = await GetRefundDetail(transaction);
                    if(paymentRefundDetail.IsProcessing)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    else
                    {
                        if (paymentRefundDetail.ReturnCode.ToString() == ZalopayReturnCode.FAIL)
                            return Result.Fail($"fail with message from paygate: {paymentRefundDetail.SubReturnMessage} and code: {paymentRefundDetail.ReturnCode} ");
                        else //if you success
                            break; 
                    }
                }
            }
            ArgumentNullException.ThrowIfNull(paymentRefundDetail);
            await _unitOfWork.BeginTransactionAsync();
            await _transactionRepository.Create(transaction);
            order.AddRefund(transaction);
            await _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return Result.Ok(paymentRefundDetail) ;
        }

        public async Task<PaymentDetail> GetTransactionDetail(Transaction payTrasactionType)
        {
            var param = new Dictionary<string, string>();
            param.Add("app_id", appid);
            param.Add("app_trans_id", payTrasactionType.AppTransactionCode);
            var data = appid + "|" + payTrasactionType.AppTransactionCode + "|" + key1;
            param.Add("mac", HmacHelper.Compute(ZaloPayHMAC.HMACSHA256, key1, data));
            var result = await HttpHelper.PostFormAsync<ZalopayTransactionResponse>(query_order_url, param);
            return new PaymentDetail() { 
                AppReceiveAmount = result.amount,
                IsProcessing = result.is_processing, 
                PaygateTransactionId = result.zp_trans_id.ToString(),
                ReturnCode = result.return_code ,
                ReturnMessage = result.return_message,
                SubReturnCode = result.sub_return_code,
                SubReturnMessage = result.sub_return_message
            };
        }

    }
}
