﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.Impact.Services
{
    /// <summary>
    /// Represents the plugin service
    /// </summary>
    public class ImpactService
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ImpactSettings _impactSettings;
        private readonly ImpactHttpClient _impactHttpClient;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IReturnRequestService _returnRequestService;

        #endregion

        #region Ctor

        public ImpactService(CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            ImpactSettings impactSettings,
            ImpactHttpClient impactHttpClient,
            IOrderService orderService,
            IProductService productService,
            IRepository<Discount> discountRepository,
            IReturnRequestService returnRequestService)
        {
            _customerSettings = customerSettings;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _discountService = discountService;
            _genericAttributeService = genericAttributeService;
            _impactSettings = impactSettings;
            _impactHttpClient = impactHttpClient;
            _orderService = orderService;
            _productService = productService;
            _discountRepository = discountRepository;
            _returnRequestService = returnRequestService;
        }

        #endregion

        #region Utils

        private async Task SendActionsAsync(Order order, OrderItem orderItem, int newCount = 0)
        {
            if (!_impactSettings.Enabled)
                return;

            var clickId = await _genericAttributeService.GetAttributeAsync<string>(order, ImpactDefaults.ClickIdAttributeName);
            if (string.IsNullOrEmpty(clickId))
                return;

            var product = await _orderService.GetProductByOrderItemIdAsync(orderItem.Id);
            var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);

            var data = new Dictionary<string, string>
            {
                //unique identifier for the action tracker (or event type) that tracked the action
                ["ActionTrackerId"] = _impactSettings.ActionTrackerId,
                //your unique identifier for the order associated with this conversion
                ["OrderId"] = order.CustomOrderNumber,
                ["Reason"] = "ORDER_UPDATE",
                ["ItemSku"] = string.IsNullOrEmpty(sku) ? product.Id.ToString() : sku,
                ["ItemQuantity"] = newCount.ToString()
            };

            await _impactHttpClient.SendRequestAsync("Actions", HttpMethod.Put, data);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create new conversion
        /// </summary>
        /// <param name="order">The order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task CreateConversionAsync(Order order)
        {
            if (!_impactSettings.Enabled)
                return;

            var clickId = await _genericAttributeService.GetAttributeAsync<Customer, string>(order.CustomerId, ImpactDefaults.ClickIdAttributeName);
            if (string.IsNullOrEmpty(clickId))
                return;

            var discount = 
                _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);

            discount += _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);

            var data = new Dictionary<string, string>
            {
                //unique identifier for the event type (or action tracker) that tracked this conversion.Functionally identical to ActionTrackerId
                ["EventTypeId"] = _impactSettings.ActionTrackerId,
                //unique case-sensitive identifier generated by impact.com of a referring click, used to construct a consumer journey
                ["ClickId"] = clickId,
                //unique identifier that you generate for the customer that converted
                ["CustomerId"] = order.CustomerId.ToString(),
                //time and date when the conversion event actually took place, in ISO 8601 format. Alternatively, submit NOW for impact.com to use the date & time when the conversion is submitted
                ["EventDate"] = "NOW",
                //unique identifier for the campaign (or program) that the conversion is associated with. This value is also known as the ProgramId
                ["CampaignId"] = _impactSettings.ProgramId,
                //your unique identifier for the order associated with this conversion
                ["OrderId"] = order.CustomOrderNumber,
                //currency code
                ["CurrencyCode"] = order.CustomerCurrencyCode,
                ["OrderDiscount"] = discount.ToString("0.00", CultureInfo.InvariantCulture)
            };
            
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            var products =
                (await _productService.GetProductsByIdsAsync(orderItems.Select(p => p.ProductId).Distinct().ToArray()))
                .ToDictionary(p => p.Id, p => p);

            for (var i = 1; i <= orderItems.Count; i++)
            {
                var item = orderItems[i - 1];
                var product = products[item.ProductId];
                var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);
                var categoryMapping = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).FirstOrDefault();
                var category = await _categoryService.GetCategoryByIdAsync(categoryMapping?.CategoryId ?? 0);

                var subTotal =
                    _currencyService.ConvertCurrency(item.PriceExclTax, order.CurrencyRate);

                data[$"ItemSku{i}"] = string.IsNullOrEmpty(sku) ? item.ProductId.ToString() : sku;
                data[$"ItemName{i}"] = product.Name;
                data[$"ItemCategory{i}"] = category?.Name ?? "No category";
                data[$"ItemSubTotal{i}"] = subTotal.ToString("0.00", CultureInfo.InvariantCulture);
                data[$"ItemQuantity{i}"] = item.Quantity.ToString();
            }

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

            //order discount coupon
            var appliedDiscounts = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);

            var appliedDiscountCouponCodes =
                (await _discountRepository.GetByIdsAsync(appliedDiscounts.Select(duh => duh.DiscountId).ToArray()))
                .Where(d => d.RequiresCouponCode).Select(p => p.CouponCode).ToList();

            if (appliedDiscountCouponCodes.Any())
                data["OrderPromoCode"] = string.Join(", ", appliedDiscountCouponCodes);

            //customer IP address
            if (_customerSettings.StoreIpAddresses && !string.IsNullOrEmpty(customer.LastIpAddress))
                data["IpAddress"] = customer.LastIpAddress;

            await _impactHttpClient.SendRequestAsync("Conversions", HttpMethod.Post, data);

            //move ClickId value to the order
            await _genericAttributeService.SaveAttributeAsync(order, ImpactDefaults.ClickIdAttributeName, clickId);
            await _genericAttributeService.SaveAttributeAsync<string>(customer, ImpactDefaults.ClickIdAttributeName, null);
        }

        /// <summary>
        /// Edit conversion
        /// </summary>
        /// <param name="returnRequest">The return request</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SendActionsAsync(ReturnRequest returnRequest)
        {
            if (!_impactSettings.Enabled)
                return;

            var order = await _orderService.GetOrderByOrderItemAsync(returnRequest.OrderItemId);

            if(order == null)
                return;

            var returnRequestAvailability =
                (await _returnRequestService.GetReturnRequestAvailabilityAsync(order.Id)).ReturnableOrderItems
                .FirstOrDefault(p => p.OrderItem.Id == returnRequest.OrderItemId);

            if (returnRequestAvailability == null) 
                return;

            await SendActionsAsync(order, returnRequestAvailability.OrderItem,
                returnRequestAvailability.AvailableQuantityForReturn);
        }

        /// <summary>
        /// Edit conversion
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SendActionsAsync(Order order)
        {
            if (!_impactSettings.Enabled)
                return;

            foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id)) 
                await SendActionsAsync(order, orderItem);
        }

        #endregion
    }
}