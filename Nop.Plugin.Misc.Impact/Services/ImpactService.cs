﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.Impact.Services
{
    /// <summary>
    /// Represents the plugin service
    /// </summary>
    public class ImpactService
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ImpactSettings _impactSettings;
        private readonly ImpactHttpClient _impactHttpClient;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;

        #endregion

        #region Ctor

        public ImpactService(ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ImpactSettings impactSettings,
            ImpactHttpClient impactHttpClient,
            IOrderService orderService,
            IProductService productService,
            IReturnRequestService returnRequestService)
        {
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _impactSettings = impactSettings;
            _impactHttpClient = impactHttpClient;
            _orderService = orderService;
            _productService = productService;
            _returnRequestService = returnRequestService;
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
                ["CurrencyCode"] = order.CustomerCurrencyCode
            };

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            var products =
                (await _productService.GetProductsByIdsAsync(orderItems.Select(p => p.ProductId).ToArray()))
                .ToDictionary(p => p.Id, p => p);

            for (var i = 1; i <= orderItems.Count; i++)
            {
                var item = orderItems[i - 1];
                var product = products[item.ProductId];
                var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);
                var categoryMapping = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).FirstOrDefault();
                var category = await _categoryService.GetCategoryByIdAsync(categoryMapping.CategoryId);

                var subTotal =
                    _currencyService.ConvertCurrency(order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax ?
                    item.PriceInclTax : item.PriceExclTax, order.CurrencyRate);

                data[$"ItemSku{i}"] = string.IsNullOrEmpty(sku) ? item.ProductId.ToString() : sku;
                data[$"ItemCategory{i}"] = category?.Name ?? "No category";
                data[$"ItemSubTotal{i}"] = subTotal.ToString(CultureInfo.InvariantCulture);
                data[$"ItemQuantity{i}"] = item.Quantity.ToString();
            }

            await _impactHttpClient.SendRequestAsync("Conversions", HttpMethod.Post, data);

            //clear ClickId value
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
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

            var returnRequestAvailability =
                (await _returnRequestService.GetReturnRequestAvailabilityAsync(order.Id)).ReturnableOrderItems
                .FirstOrDefault(p => p.OrderItem.Id == returnRequest.OrderItemId);

            var product = await _orderService.GetProductByOrderItemIdAsync(returnRequest.OrderItemId);
            var sku = await _productService.FormatSkuAsync(product, returnRequestAvailability?.OrderItem?.AttributesXml);

            var data = new Dictionary<string, string>
            {
                //unique identifier for the action tracker (or event type) that tracked the action
                ["ActionTrackerId"] = _impactSettings.ActionTrackerId,
                //your unique identifier for the order associated with this conversion
                ["OrderId"] = order.CustomOrderNumber.ToString(),
                ["Reason"] = "ORDER_UPDATE",
                ["ItemSku"] = string.IsNullOrEmpty(sku) ? product.Id.ToString() : sku,
                ["ItemQuantity"] = (returnRequestAvailability?.AvailableQuantityForReturn ?? 0).ToString()
            };

            await _impactHttpClient.SendRequestAsync("Actions", HttpMethod.Put, data);
        }

        #endregion
    }
}