using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuttingEdge.Conditions;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.BillingService;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.BusinessProcess;
using InfiniteSynergy.MobileForHero.BusinessServices.Models.Business;
using System.Net.Http;
using InfinitySynergy.Utility.Patterns;
using InfinitySynergy.Utility.Types;
using RestSharp;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.BillingService.BeginPayment;
using InfiniteSynergy.Utility;
using Newtonsoft.Json;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.BillingService.Notify;
using InfinitySynerge.MobileForHero.BusinessServices.Billing.DTO;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Repository.Business;
using InfiniteSynergy.MobileForHero.DataAccess.ModelContracts.Identifiers;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Other;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService.CompleteOrder;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Repository.Billing;
using InfiniteSynergy.MobileForHero.BusinessServices.Models.Billing;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.Communication;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.Common;
using InfiniteSynergy.MobileForHero.Common;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.BillingService.CheckPayment;
using InfiniteSynergy.MobileForHero.DataAccess.ModelContracts.Enumerations;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService.RefundOrder;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.Communication;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService;
using InfiniteSynergy.MobileForHero.BusinessServices.Common;

namespace InfinitySynerge.MobileForHero.BusinessServices.Billing
{
    public class TinkoffBillingService : IBillingService
    {
        private class TinkoffPaymentStatus
        {
            public const string Authorized = "AUTHORIZED";
            public const string Confirmed = "CONFIRMED";
            public const string Reversed = "REVERSED";
            public const string Refunded = "REFUNDED";
            public const string PartialRefunded = "PARTIAL_REFUNDED";
            public const string Rejected = "REJECTED";
        }

        private const int SingleProduct = 1;

        private const string VAT_None = "none";
        private const string Taxation = "usn_income";
        private const string LanguageRU = "ru";

        private const string MerchantApiUrl = "https://securepay.tinkoff.ru/v2";

        private readonly IOrderService _orderService;
        private readonly IOrderRepository _orderRepository;

        private readonly IEmailOrchestrationService _emailOrchestrationService;

        private readonly IBillingEntryRepository _billingEntryRepository;

        private TerminalOptions TerminalOptions { get; set; }

        public TinkoffBillingService(
            IEmailOrchestrationService emailOrchestrationService,
            IOrderService orderService, 
            IOrderRepository orderRepository,
            IBillingEntryRepository billingEntryRepository,
            ISystemSettingsService systemSettingsService)
        {
            Condition.Requires(orderService, nameof(orderService)).IsNotNull();
            Condition.Requires(orderService, nameof(orderService)).IsNotNull();
            Condition.Requires(orderRepository, nameof(orderRepository)).IsNotNull();
            Condition.Requires(billingEntryRepository, nameof(billingEntryRepository)).IsNotNull();
            Condition.Requires(systemSettingsService, nameof(systemSettingsService)).IsNotNull();
            Condition.Requires(emailOrchestrationService, nameof(emailOrchestrationService)).IsNotNull();

            _emailOrchestrationService = emailOrchestrationService;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _billingEntryRepository = billingEntryRepository;

            TerminalOptions = new TerminalOptions(
                systemSettingsService.Read<string>(SSP.Billing_TerminalKey).Result,
                systemSettingsService.Read<string>(SSP.Billing_TerminalPassword).Result
            );
        }

        public async Task<MethodResult<BeginPaymentResponse>> BeginPaymentAsync(BeginPaymentRequest request)
        {
            return await SafeContext.ExecuteActionAsync<BeginPaymentResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                var readOrderResponse = await _orderService.ReadByIdAsync(request.OrderId);
                if (readOrderResponse.Ok)
                {
                    var order = readOrderResponse.Result;
                    var orderPriceWithDiscount = order.PriceWithDiscount;

                    var initPaymentRequest = new TinkoffPayment_InitRequest()
                    {
                        TerminalKey = TerminalOptions.Key,
                        OrderId = request.OrderId.ToString(),
                        IP = request.CustomerIP,
                        Language = LanguageRU,
                        Amount = ToKopek(orderPriceWithDiscount.TotalPrice),
                        Description = BuildOrderDescription(order),
                        Receipt = new TinkoffPayment_ReceiptModel()
                        {
                            Email = order.Email,
                            Phone = order.PhoneNumber,
                            Taxation = Taxation,
                            Items = order.OrderDetails.Select(orderDetail => new TinkoffPayment_ReceiptItemModel()
                            {
                                Amount = ToKopek(orderPriceWithDiscount.DetailsPrice[orderDetail.Id]),
                                Name = BuildOrderDetailDescription(orderDetail),
                                Price = ToKopek(orderPriceWithDiscount.DetailsPrice[orderDetail.Id]),
                                Quantity = SingleProduct,
                                Tax = VAT_None
                            })
                        }
                    };

                    initPaymentRequest.Signify(TerminalOptions.Password);

                    var httpClient = new RestClient(MerchantApiUrl);

                    var initRequest = new RestRequest("/Init", Method.POST);
                    initRequest.AddParameter("application/json", initPaymentRequest.Serialize(), ParameterType.RequestBody);

                    var response = httpClient.Execute<TinkoffPayment_InitResponse>(initRequest);

                    if (response.IsSuccessful)
                    {
                        var initResponse = response.Data;
                        if (initResponse.Success)
                        {
                            await _emailOrchestrationService.SendPaymentLinkNotificationEmailAsync(
                                new SendPaymentLinkNotificationEmailRequest(
                                    order.CustomerFullName, order.Email, order.Id, initResponse.PaymentURL
                                )
                            );

                            await _billingEntryRepository.CreateAsync(BuildCreateBillingEntryRequest(initResponse));

                            methodResult.Result = new BeginPaymentResponse(initResponse.PaymentURL);
                        }
                        else
                        {
                            methodResult.AddFatalError(JsonConvert.SerializeObject(initResponse));

                            await _emailOrchestrationService.SendServiceEmailAsync(
                                new SendServiceEmailRequest(ErrorMessages.BuildUnexpectedErrorMessage(order.Id), methodResult.ToString())
                            );
                        }
                    }
                    else
                    {
                        methodResult.AddCommonError(ErrorMessages.BuildBankInnerError());
                    }
                }
                else
                {
                    methodResult.AddCommonError(ErrorMessages.BuildOrderNotFountError(request.OrderId));
                }
            });
        }

        public async Task<MethodResult<NotifyResponse>> NotifyAsync(NotifyRequest request)
        {
            return await SafeContext.ExecuteActionAsync<NotifyResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                methodResult.Result = new NotifyResponse(isSuccessfulPayment: false);

                var orderId = long.Parse(request.OrderId);
                if (request.Verify(TerminalOptions.Password))
                {
                    await _billingEntryRepository.CreateAsync(BuildCreateBillingEntryRequest(request));

                    var readOrderResponse = await _orderService.ReadByIdAsync(orderId);
                    if (readOrderResponse.Ok)
                    {
                        var order = readOrderResponse.Result;

                        switch (request.Status)
                        {
                            case TinkoffPaymentStatus.Authorized:
                                break;
                            case TinkoffPaymentStatus.Confirmed:
                                await _orderRepository.UpdatePaymentStatusByIdAsync(order.Id, PaymentStatus.Paid);
                                await _orderRepository.UpdateStatusByIdAsync(order.Id, OrderStatusIdentifier.InProductionProgress);

                                var processOrderResponse = await _orderService.CompleteOrderAsync(new ProcessOrderRequest(order.Id));
                                if (processOrderResponse.Ok)
                                {
                                    methodResult.Result.IsSuccessfulPayment = true;

                                    await _emailOrchestrationService.SendSuccessfullPaymentEmailAsync(
                                        new SendSuccessfulPaymentEmailRequest(order.Email, order.CustomerFullName, order.Id)
                                    );
                                }
                                else
                                {
                                    await _orderRepository.UpdateStatusByIdAsync(order.Id, OrderStatusIdentifier.UnableToProcessOrder);
                                    methodResult.MergeWith(processOrderResponse);

                                    await _emailOrchestrationService.SendServiceEmailAsync(
                                        new SendServiceEmailRequest(ErrorMessages.BuildProcessOrderServiceMessage(order.Id), methodResult.ToString())
                                    );
                                }

                                break;

                            case TinkoffPaymentStatus.Rejected:
                                await _orderRepository.UpdatePaymentStatusByIdAsync(order.Id, PaymentStatus.Rejected);
                                break;

                            case TinkoffPaymentStatus.Refunded:
                                await _orderRepository.UpdatePaymentStatusByIdAsync(order.Id, PaymentStatus.Refunded);

                                var refundOrderResponse = await _orderService.RefundOrderAsync(new RefundOrderRequest(order.Id));
                                if (refundOrderResponse.Ok)
                                {
                                    await _emailOrchestrationService.SendRefundOrderEmailAsync(
                                        new SendRefundOrderEmailRequest(order.Email, order.CustomerFullName, order.Id)
                                    );
                                }
                                else
                                {
                                    methodResult.MergeWith(refundOrderResponse);

                                    await _emailOrchestrationService.SendServiceEmailAsync(
                                        new SendServiceEmailRequest(ErrorMessages.BuildRefundOrderErrorMessage(order.Id), methodResult.ToString())
                                    );
                                }

                                break;

                            default:
                                methodResult.AddCommonError(ErrorMessages.BuildIncorrectPaymentStatusResponse(order.Id));
                                break;
                        }
                    }
                    else
                    {
                        methodResult.MergeWith(readOrderResponse);

                        await _emailOrchestrationService.SendServiceEmailAsync(
                            new SendServiceEmailRequest(ErrorMessages.BuildReadOrderErrorServiceMessage(orderId), methodResult.ToString())
                        );
                    }
                }
                else
                {
                    methodResult.AddCommonError(ErrorMessages.BuildNotificationVerifyError(orderId));

                    await _emailOrchestrationService.SendServiceEmailAsync(
                        new SendServiceEmailRequest(ErrorMessages.BuildNotificationVerifyError(orderId), methodResult.ToString())
                    );
                }
            });
        }

        public async Task<MethodResult<CheckPaymentResponse>> CheckPaymentAsync(CheckPaymentRequest request)
        {
            return await SafeContext.ExecuteActionAsync<CheckPaymentResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                var readOrderResponse = await _orderService.ReadByIdAsync(request.OrderId);
                if (readOrderResponse.Ok)
                {
                    var isOrderActuallyPaid = false;

                    var order = readOrderResponse.Result;
                    if (order.PaymentStatus == PaymentStatus.Paid)
                    {
                        isOrderActuallyPaid = true;
                    }
                    else
                    {
                        await _emailOrchestrationService.SendServiceEmailAsync(
                            new SendServiceEmailRequest(ErrorMessages.BuildAttemptToCheckUnpaidOrderServiceMessage(order.Id), methodResult.ToString())
                        );
                    }

                    methodResult.Result = new CheckPaymentResponse(isOrderActuallyPaid);
                }
                else
                {
                    methodResult.AddCommonError(ErrorMessages.BuildOrderNotFountError(request.OrderId));
                }
            });
        }

        

        private long ToKopek(decimal value)
        {
            var left = Math.Floor(value);
            var right = (long)Math.Ceiling(Math.Abs(value - left));

            return (long)left * 100 + right;
        }

        private CreateBillingEntryRequest BuildCreateBillingEntryRequest(TinkoffPayment_InitResponse initResponse)
        {
            return new CreateBillingEntryRequest()
            {
                Amount = initResponse.Amount,
                Details = initResponse.Details,
                ErrorCode = initResponse.ErrorCode,
                Data = null,
                Message = initResponse.Message,
                OrderId = long.Parse(initResponse.OrderId),
                PaymentId = initResponse.PaymentId,
                Status = initResponse.Status,
                TerminalKey = initResponse.TerminalKey
            };
        }

        private CreateBillingEntryRequest BuildCreateBillingEntryRequest(NotifyRequest finishPaymentRequest)
        {
            return new CreateBillingEntryRequest()
            {
                Amount = finishPaymentRequest.Amount,
                Details = null,
                ErrorCode = finishPaymentRequest.ErrorCode,
                Data = finishPaymentRequest.DATA == null 
                    ? null 
                    : JsonConvert.SerializeObject(finishPaymentRequest.DATA),
                Message = null,
                OrderId = long.Parse(finishPaymentRequest.OrderId),
                PaymentId = finishPaymentRequest.PaymentId,
                Status = finishPaymentRequest.Status,
                TerminalKey = finishPaymentRequest.TerminalKey
            };
        }

        //...
    }
}
