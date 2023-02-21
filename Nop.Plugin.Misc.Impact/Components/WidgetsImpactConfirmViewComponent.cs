using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Impact.Components
{
    /// <summary>
    /// Represents the view component to save ClickId parameter for a customer
    /// </summary>
    public class WidgetsImpactConfirmViewComponent : NopViewComponent
    {
        #region Fields

        private IGenericAttributeService _genericAttributeService;
        private IHttpContextAccessor _httpContextAccessor;
        private IWebHelper _webHelper;
        private IWorkContext _workContext;
        private ImpactSettings _impactSettings;

        #endregion

        #region Ctor

        public WidgetsImpactConfirmViewComponent(IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            IWebHelper webHelper,
            IWorkContext workContext,
            ImpactSettings impactSettings)
        {
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
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

            //whether the value is already stored
            var clickId = await _genericAttributeService.GetAttributeAsync<string>(customer, ImpactDefaults.ClickIdAttributeName);
            if (!string.IsNullOrEmpty(clickId))
                return Content(string.Empty);

            //try to get the value from query parameters
            clickId = _webHelper.QueryString<string>(ImpactDefaults.ClickIdQueryParamName);
            if (!string.IsNullOrEmpty(clickId))
            {
                await _genericAttributeService.SaveAttributeAsync(customer, ImpactDefaults.ClickIdAttributeName, clickId);

                return Content(string.Empty);
            }

            //try to get the value from cookies
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Request.Cookies.TryGetValue($"{ImpactDefaults.ClickIdQueryCookiePrefix}{_impactSettings.ProgramId}", out var cookie) && !string.IsNullOrEmpty(cookie))
            {
                clickId = cookie.Trim('|').Split('|').Last();
                if (!string.IsNullOrEmpty(clickId))
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, ImpactDefaults.ClickIdAttributeName, clickId);

                    return Content(string.Empty);
                }
            }

            //try to get the value from the client script
            return View("~/Plugins/Misc.Impact/Views/PublicConfirmInfo.cshtml");
        }

        #endregion
    }
}