using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.Impact.Services
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<OrderPaidEvent>,
        IConsumer<EntityUpdatedEvent<ReturnRequest>>,
        IConsumer<EntityDeletedEvent<Order>>,
        IConsumer<OrderStatusChangedEvent>
    {
        #region Fields

        private readonly ImpactService _impactService;

        #endregion

        #region Ctor

        public EventConsumer(ImpactService impactService)
        {
           _impactService = impactService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            await _impactService.CreateConversionAsync(eventMessage.Order);
        }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ReturnRequest> eventMessage)
        {
            var entity = eventMessage.Entity;
            if (entity.ReturnRequestStatus != ReturnRequestStatus.ItemsRefunded)
                return;

            await _impactService.SendActionsAsync(entity);
        }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
        {
            var order = eventMessage.Order;

            if (eventMessage.PreviousOrderStatus == order.OrderStatus)
                return;

            if(order.PaymentStatus == PaymentStatus.Paid && order.OrderStatus == OrderStatus.Cancelled)
                await _impactService.SendActionsAsync(order);
        }

        /// <summary>
        /// Handle event
        /// </summary>
        /// <param name="eventMessage">Event</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
        {
            var order = eventMessage.Entity;

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Paid)
                return;

            await _impactService.SendActionsAsync(order);
        }

        #endregion
    }
}