using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Impact.Components
{
    /// <summary>
    /// Represents the view component to place the Universal Tracking Script
    /// </summary>
    public class WidgetsImpactHeadViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private IEncryptionService _encryptionService;
        private IWorkContext _workContext;
        private ImpactSettings _impactSettings;

        #endregion

        #region Ctor

        public WidgetsImpactHeadViewComponent(ICustomerService customerService,
            IEncryptionService encryptionService,
            IWorkContext workContext,
            ImpactSettings impactSettings)
        {
            _customerService = customerService;
            _encryptionService = encryptionService;
            _workContext = workContext;
            _impactSettings = impactSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_impactSettings.Enabled)
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerEmail = !await _customerService.IsGuestAsync(customer)
                ? customer.Email?.Replace("'", "\\'")
                : string.Empty;

            var script = new StringBuilder(_impactSettings.UniversalTrackingScript);

            var emailHash = _encryptionService.CreatePasswordHash(customerEmail, string.Empty, ImpactDefaults.HashAlgorithm);

            script.Replace("customerid: ''", $"customerid: '{customer.Id}'");
            script.Replace("customeremail: ''", $"customeremail: '{emailHash}'");

            return new HtmlContentViewComponentResult(new HtmlString(script.ToString()));
        }

        #endregion
    }
}