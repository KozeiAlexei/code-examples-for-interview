using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.BusinessProcess;
using InfinitySynergy.Utility.Types;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Repository.Product;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Repository.Business;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Repository.Cases;
using InfinitySynergy.Utility;
using InfiniteSynergy.MobileForHero.DataAccess.Contracts.Other;
using InfinitySynergy.Utility.Patterns;
using InfiniteSynergy.MobileForHero.BusinessServices.Models.Business;
using InfiniteSynergy.MobileForHero.DataAccess.ModelContracts.Identifiers;
using CuttingEdge.Conditions;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.DataManagment;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.DataManagment.PhoneModelColoredBlankService;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.DataManagment.BlankService;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.Common;
using InfiniteSynergy.MobileForHero.Common;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using InfiniteSynergy.MobileForHero.BusinessServices.Common;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService.CompleteOrder;
using InfiniteSynergy.MobileForHero.DataAccess.ModelContracts.Enumerations;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.DataManagment.BlankService.BuildSoldMap;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService.RefundOrder;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.DataManagment.BlankService.UpdateStatus;
using InfiniteSynergy.MobileForHero.BusinessServices.Contracts.Communication;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.Communication;
using Newtonsoft.Json;
using InfiniteSynergy.MobileForHero.BusinessServices.ActionParams.BusinessProcess.OrderService.UpdateFromCart;

namespace InfinitySynerge.MobileForHero.BusinessServices.BusinesProcess
{
    public class OrderService : IOrderService
    {
        #region Constructor

        private readonly IDbHelper _dbHelper;
        private readonly IOrderRepository _orderRepository;
        private readonly IBookingCaseRepository _bookingCaseRepository;

        private readonly IEmailOrchestrationService _emailOrchestrationService;
        private readonly IBlankService _blankService;
        private readonly ISystemSettingsService _settingsService;
        private readonly IPhoneModelColoredBlankService _phoneModelColoredBlankService;
        private readonly IDeliveryMethodRepository _deliveryMethodRepository;
        private readonly IDiscountCouponeService _discountCouponeService;

        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public OrderService(
            IDbHelper dbHelper,
            IOrderRepository orderRepository,
            IBookingCaseRepository bookingCaseRepository,
            IBlankService blankService,
            IEmailOrchestrationService emailOrchestrationService,
            ISystemSettingsService settingsService,
            IPhoneModelColoredBlankService phoneModelColoredBlankService,
            IDeliveryMethodRepository deliveryMethodRepository,
            IDiscountCouponeService discountCouponeService
            )
        {
            Condition.Requires(dbHelper, nameof(dbHelper)).IsNotNull();
            Condition.Requires(orderRepository, nameof(orderRepository)).IsNotNull();
            Condition.Requires(bookingCaseRepository, nameof(bookingCaseRepository)).IsNotNull();

            Condition.Requires(blankService, nameof(blankService)).IsNotNull();
            Condition.Requires(emailOrchestrationService, nameof(emailOrchestrationService)).IsNotNull();
            Condition.Requires(settingsService, nameof(settingsService)).IsNotNull();
            Condition.Requires(phoneModelColoredBlankService, nameof(phoneModelColoredBlankService)).IsNotNull();
            Condition.Requires(deliveryMethodRepository, nameof(deliveryMethodRepository)).IsNotNull();
            Condition.Requires(discountCouponeService, nameof(discountCouponeService)).IsNotNull();

            _dbHelper = dbHelper;
            _orderRepository = orderRepository;
            _bookingCaseRepository = bookingCaseRepository;

            _blankService = blankService;
            _emailOrchestrationService = emailOrchestrationService;
            _settingsService = settingsService;
            _phoneModelColoredBlankService = phoneModelColoredBlankService;
            _deliveryMethodRepository = deliveryMethodRepository;
            _discountCouponeService = discountCouponeService;
        }

        #endregion

        public async Task<MethodResult<IEnumerable<OrderBL>>> ReadAsync()
        {
            return await SafeContext.ExecuteWithResultAsync(
                async () => await _orderRepository.ReadAsync()
            );
        }

        public async Task<MethodResult<OrderBL>> ReadByIdAsync(long id)
        {
            return await SafeContext.ExecuteActionAsync<OrderBL>(async (methodResult) =>
            {
                var order = await _orderRepository.ReadByIdAsync(id);
                if (order == null)
                {
                    methodResult.AddCommonError(ErrorMessages.OrderNotFound(id));
                    return;
                }

                methodResult.Result = order;
            });
        }

        public async Task<MethodResult> UpdateAsync(OrderBL request)
        {
            return await SafeContext.ExecuteActionAsync(async () => await _orderRepository.UpdateAsync(request));
        }

        public async Task<MethodResult<CreateOrderResponse>> CreateAsync(CreateOrderRequest request)
        {
            return await SafeContext.ExecuteActionAsync<CreateOrderResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                // Add validation

                request.StatusId = OrderStatusIdentifier.Created;

                await _semaphoreSlim.WaitAsync();
                try
                {
                    var response = new CreateOrderResponse();

                    var backendPrice = await this.CalculateTotalOrderPrice(request);
                    if (Math.Abs(backendPrice - request.TotalPriceWithDiscount) > 10 )//fix to cover differences between js evaluated price and backend price. 10 rubles difference allowed
                    {
                        methodResult.AddCommonError(ErrorMessages.Order_TotalPriceNotEqual);
                        return;
                    }

                    using (var transaction = _dbHelper.BeginTransaction())
                    {
                        try
                        {
                            // Create order with Status = Created and PaymentStatus = WaitingForPayment:
                            response.OrderId = await _orderRepository.CreateAsync(request);

                            var order = await _orderRepository.ReadByIdAsync(response.OrderId);

                            if (request.PaymentMethodId == PaymentMethodIdentifier.UponReceipt)
                            {
                                await _orderRepository.UpdateStatusByIdAsync(response.OrderId, OrderStatusIdentifier.InProductionProgress);

                                var processOrderResponse = await CompleteOrderAsync(new ProcessOrderRequest(response.OrderId));
                                if (processOrderResponse.HasErrors)
                                {
                                    methodResult.MergeWith(processOrderResponse);
                                }                     
                            }
                            else
                            {
                                var bookingBlanksByOrderResponse = await _blankService.UpdateStatusAsync(
                                    new UpdateStatusRequest(order.OrderDetails.Select(orderDetail => orderDetail.Product.Id), BlankStatus.OrderCompletionAwaiting)
                                );

                                if (bookingBlanksByOrderResponse.HasErrors)
                                {
                                    methodResult.MergeWith(bookingBlanksByOrderResponse);
                                    methodResult.AddCommonError(ErrorMessages.Order_UnableToProcessOrder);

                                    await _emailOrchestrationService.SendServiceEmailAsync(
                                        new SendServiceEmailRequest(
                                            ErrorMessages.BuildUnableToCreateOrder(order.Id),
                                            methodResult.ToString() + "\n \n" + JsonConvert.SerializeObject(order)
                                        )
                                    );
                                }
                            }

                            methodResult.Result = response;

                            if (methodResult.Ok)
                            {
                                transaction.Commit();

                                await _emailOrchestrationService.SendOrderDetailsEmailAsync(
                                    new SendOrderDetailsEmailRequest(order, order.Email)
                                );

                                await _emailOrchestrationService.SendMakeOrderNotificationEmailAsync(
                                    new SendMakeOrderNotificationEmailRequest(BuildOrderViewUrl(order.Id))
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            methodResult.AddFatalError(ex);
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            });
        }

        public async Task<MethodResult<UpdateFromCartResponse>> UpdateFromCartAsync(UpdateFromCartRequest request)
        {
            return await SafeContext.ExecuteActionAsync<UpdateFromCartResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                // Add validation

                request.StatusId = OrderStatusIdentifier.Created;

                await _semaphoreSlim.WaitAsync();
                try
                {
                    var response = new UpdateFromCartResponse();

                    var backendPrice = await this.CalculateTotalOrderPrice(request);
                    if (Math.Abs(backendPrice - request.TotalPriceWithDiscount) > 10)//fix to cover differences between js evaluated price and backend price. 10 rubles difference allowed
                    {
                        methodResult.AddCommonError(ErrorMessages.Order_TotalPriceNotEqual);
                        return;
                    }

                    using (var transaction = _dbHelper.BeginTransaction())
                    {
                        try
                        {
                            var order = await _orderRepository.ReadByIdAsync(request.Id);
                            if (order.Status.Id != (long)OrderStatusIdentifier.Created)
                            {
                                methodResult.AddCommonError(ErrorMessages.Order_UnableToProcessOrder);
                                return;
                            }

                            await _orderRepository.UpdateFromCartAsync(request);

                            if (request.PaymentMethodId == PaymentMethodIdentifier.UponReceipt)
                            {
                                await _orderRepository.UpdateStatusByIdAsync(order.Id, OrderStatusIdentifier.InProductionProgress);

                                var processOrderResponse = await CompleteOrderAsync(new ProcessOrderRequest(order.Id));
                                if (processOrderResponse.HasErrors)
                                {
                                    methodResult.MergeWith(processOrderResponse);
                                }
                            }
                            else
                            {
                                var bookingBlanksByOrderResponse = await _blankService.UpdateStatusAsync(
                                    new UpdateStatusRequest(order.OrderDetails.Select(orderDetail => orderDetail.Product.Id), BlankStatus.OrderCompletionAwaiting)
                                );

                                if (bookingBlanksByOrderResponse.HasErrors)
                                {
                                    methodResult.MergeWith(bookingBlanksByOrderResponse);
                                    methodResult.AddCommonError(ErrorMessages.Order_UnableToProcessOrder);

                                    await _emailOrchestrationService.SendServiceEmailAsync(
                                        new SendServiceEmailRequest(
                                            ErrorMessages.BuildUnableToCreateOrder(order.Id),
                                            methodResult.ToString() + "\n \n" + JsonConvert.SerializeObject(order)
                                        )
                                    );
                                }
                            }

                            methodResult.Result = response;

                            if (methodResult.Ok)
                            {
                                transaction.Commit();

                                await _emailOrchestrationService.SendOrderDetailsEmailAsync(
                                    new SendOrderDetailsEmailRequest(order, order.Email)
                                );
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            methodResult.AddFatalError(ex);
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            });
        }

        public async Task<MethodResult<CompleteOrderResponse>> CompleteOrderAsync(ProcessOrderRequest request)
        {
            return await SafeContext.ExecuteActionAsync<CompleteOrderResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                var order = await _orderRepository.ReadByIdAsync(request.OrderId);

                // Mark product blanks as sold:
                var updateStatus = await _blankService.UpdateStatusAsync(new UpdateStatusRequest(
                    order.OrderDetails.Select(orderDetail => orderDetail.Product.Id),
                    BlankStatus.Sold
                ));

                if (updateStatus.HasErrors)
                {
                    methodResult.AddCommonError(ErrorMessages.Order_UnableToProcessOrder);
                }
            });
        }

        public async Task<MethodResult<RefundOrderResponse>> RefundOrderAsync(RefundOrderRequest request)
        {
            return await SafeContext.ExecuteActionAsync<RefundOrderResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                var order = await _orderRepository.ReadByIdAsync(request.OrderId);

                // Mark product blanks as having undefined state:
                var updateStatus = await _blankService.UpdateStatusAsync(new UpdateStatusRequest(
                    order.OrderDetails.Select(orderDetail => orderDetail.Product.Id),
                    BlankStatus.Undefined
                ));

                if (updateStatus.HasErrors)
                {
                    methodResult.AddCommonError(ErrorMessages.Order_UnableToProcessOrder);
                }
            });
        }

        public async Task<MethodResult<BookingCaseResponse>> BookingCaseAsync(BookingCaseRequest request)
        {
            return await SafeContext.ExecuteActionAsync<BookingCaseResponse>(async (methodResult) =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                var bookingCaseResponse = new BookingCaseResponse();

                await _semaphoreSlim.WaitAsync();
                try
                {
                    var expiresDate = default(DateTime);

                    var readBookingDurationCountResponse = _settingsService.Read<int>(MainSettings_Keys.BookingDurationMinutes);
                    if (readBookingDurationCountResponse.Ok)
                    {
                        expiresDate = DateTime.Now.AddMinutes(readBookingDurationCountResponse.Result);
                    }

                    using (var transaction = _dbHelper.BeginTransaction())
                    {
                        try
                        {
                            if (request.ExistingBookingId.HasValue)
                            {
                                await _bookingCaseRepository.DeleteAsync(request.ExistingBookingId.Value);
                            }

                            var allowedBookingRequest = new BookingCaseRequest(expiresDate);
                            var buildSoldMapRequest = new BuildSoldMapRequest(request.Entries.Select(entry => entry.BlankId));

                            var buildSoldMapResponse = await _blankService.BuildSoldMapAsync(buildSoldMapRequest);
                            if (buildSoldMapResponse.HasErrors)
                            {
                                methodResult.MergeWith(buildSoldMapResponse);
                                return;
                            }

                            var soldMap = buildSoldMapResponse.Result.SoldMap;
                            foreach (var entryForBooking in request.Entries)
                            {
                                if (soldMap[entryForBooking.BlankId] == true)
                                {
                                    entryForBooking.IsAlreadySold = true;
                                    bookingCaseResponse.NotAllowedEntries.Add(entryForBooking);
                                    continue;
                                }

                                var bookingForBlankAllowed =
                                    await _bookingCaseRepository.WasBlankBookedAsync(entryForBooking.BlankId);

                                //var phoneModelColoredBlankIgnoredCount =
                                //    allowedBookingRequest.Entries.Count(entry => entry.PhoneModelColoredBlankId == entryForBooking.PhoneModelColoredBlankId);

                                //var bookingForPhoneModelColoredBlankAllowed =
                                //    await _bookingCaseRepository.WasPhoneModelColoredBlankBookedAsync(entryForBooking.PhoneModelColoredBlankId, phoneModelColoredBlankIgnoredCount);

                                if (bookingForBlankAllowed/* && bookingForPhoneModelColoredBlankAllowed*/)
                                {
                                    allowedBookingRequest.Entries.Add(entryForBooking);
                                }
                                else
                                {
                                    bookingCaseResponse.NotAllowedEntries.Add(entryForBooking);
                                }
                            }

                            if (allowedBookingRequest.Entries.Any())
                            {
                                bookingCaseResponse.BookingId = await _bookingCaseRepository.CreateAsync(allowedBookingRequest);
                            }

                            transaction.Commit();

                            methodResult.Result = bookingCaseResponse;
                        }
                        catch (Exception exception)
                        {
                            transaction.Rollback();
                            methodResult.AddFatalError(exception);
                        }
                        finally
                        {
                            transaction.Dispose();
                        }
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            });
        }

        public async Task<MethodResult> RemoveBookingAsync(RemoveBookingRequest request)
        {
            return await SafeContext.ExecuteActionAsync(async () =>
            {
                Condition.Requires(request, nameof(request)).IsNotNull();

                await _semaphoreSlim.WaitAsync();
                try
                {
                    if (request.BlankIds != null && request.BlankIds.Any())
                    {
                        await _bookingCaseRepository.DeleteEntriesByBlankIds(request.BookingId, request.BlankIds);
                    }
                    else
                    {
                        await _bookingCaseRepository.DeleteAsync(request.BookingId);
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            });
        }

        private string BuildOrderViewUrl(long orderId)
        {
            // It was hide for review:
            return $"...";
        }

        private MarkBlanksAsSoldRequest BuildMarkBlanksAsSoldRequest(CreateOrderRequest createOrderRequest)
        {
            return new MarkBlanksAsSoldRequest()
            {
                BlankIdentifiers = createOrderRequest.Details.Select(detail => detail.BlankId)
            };
        }

        private ChangeStockCountRequest BuildUpdateStockCountRequest(CreateOrderRequest createOrderRequest)
        {
            var request = new ChangeStockCountRequest();
            foreach (var detail in createOrderRequest.Details)
            {
                if (request.Changes.ContainsKey(detail.PhoneModelColoredBlankId))
                    request.Changes[detail.PhoneModelColoredBlankId]++;
                else
                    request.Changes[detail.PhoneModelColoredBlankId] = 1;
            }

            return request;
        }

        private ChangeStockCountRequest BuildUpdateStockCountRequest(OrderBL order)
        {
            var request = new ChangeStockCountRequest();
            foreach (var orderDetail in order.OrderDetails)
            {
                var coloredBlankId = orderDetail.PhoneModelColoredBlank.Id;

                if (request.Changes.ContainsKey(coloredBlankId))
                    request.Changes[coloredBlankId]++;
                else
                    request.Changes[coloredBlankId] = 1;
            }

            return request;
        }

        private async Task<decimal> CalculateTotalOrderPrice(CreateOrderRequest createOrderRequest)
        {
            decimal totalPrice = 0;
            foreach (var currentItem in createOrderRequest.Details)
            {
                var currentBlank = await _blankService.ReadByIdAsync(currentItem.BlankId);
                totalPrice += currentBlank.Result.Cost;
            }

            var currentDeliveryMethod = await _deliveryMethodRepository.ReadByIdAsync(createOrderRequest.DeliveryMethodId);

            totalPrice += currentDeliveryMethod.Cost;

            if (createOrderRequest.DiscountCouponeId.HasValue)
            {
                var currentDiscountCoupone = await _discountCouponeService.ReadByIdAsync(createOrderRequest.DiscountCouponeId.Value);
                totalPrice = Math.Round(totalPrice * (1.0m - currentDiscountCoupone.Result.Discount));
            }
            return totalPrice;
        }

        private async Task<decimal> CalculateTotalOrderPrice(UpdateFromCartRequest request)
        {
            decimal totalPrice = 0;
            foreach (var currentItem in request.Details)
            {
                var currentBlank = await _blankService.ReadByIdAsync(currentItem.BlankId);
                totalPrice += currentBlank.Result.Cost;
            }

            var currentDeliveryMethod = await _deliveryMethodRepository.ReadByIdAsync(request.DeliveryMethodId);

            totalPrice += currentDeliveryMethod.Cost;

            if (request.DiscountCouponeId.HasValue)
            {
                var currentDiscountCoupone = await _discountCouponeService.ReadByIdAsync(request.DiscountCouponeId.Value);
                totalPrice = Math.Round(totalPrice * (1.0m - currentDiscountCoupone.Result.Discount));
            }
            return totalPrice;
        }

    }
}
