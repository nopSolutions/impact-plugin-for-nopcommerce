using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.Impact.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Impact.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class ImpactAdminController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly ImpactSettings _impactSettings;

        #endregion

        #region Ctor

        public ImpactAdminController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            ImpactSettings impactSettings)
        {

            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _impactSettings = impactSettings;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                Enabled = _impactSettings.Enabled,
                AccountSId = _impactSettings.AccountSId,
                AuthToken = _impactSettings.AuthToken,
                ProgramId = _impactSettings.ProgramId,
                ActionTrackerId = _impactSettings.ActionTrackerId,
                UniversalTrackingScript = _impactSettings.UniversalTrackingScript,
            };

            return View("~/Plugins/Misc.Impact/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            _impactSettings.Enabled = model.Enabled;
            _impactSettings.AccountSId = model.AccountSId;
            _impactSettings.AuthToken = model.AuthToken;
            _impactSettings.ProgramId = model.ProgramId;
            _impactSettings.ActionTrackerId = model.ActionTrackerId;
            _impactSettings.UniversalTrackingScript = model.UniversalTrackingScript;

            await _settingService.SaveSettingAsync(_impactSettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion
    }
}