using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.Impact.Services
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<OrderPaidEvent>,
        IConsumer<EntityUpdatedEvent<ReturnRequest>>
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

        #endregion
    }
}