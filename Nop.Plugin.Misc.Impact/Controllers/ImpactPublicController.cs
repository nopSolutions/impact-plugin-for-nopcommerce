using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Impact.Controllers
{
    public class ImpactPublicController : BasePluginController
    {
        #region Fields

        private HtmlEncoder _htmlEncoder;
        private IGenericAttributeService _genericAttributeService;
        private IWorkContext _workContext;

        #endregion

        #region Ctor

        public ImpactPublicController(HtmlEncoder htmlEncoder,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext)
        {
            _htmlEncoder = htmlEncoder;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        [HttpPost]
        [CheckLanguageSeoCode(ignore: true)]
        public async Task<IActionResult> SetClickId(string clickId)
        {
            if (string.IsNullOrEmpty(clickId))
                return Ok();

            var customer = await _workContext.GetCurrentCustomerAsync();
            await _genericAttributeService.SaveAttributeAsync(customer, ImpactDefaults.ClickIdAttributeName, _htmlEncoder.Encode(clickId));

            return Ok();
        }

        #endregion
    }
}