using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.Impact.Components;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.Impact
{
    /// <summary>
    /// Represents Impact plugin
    /// </summary>
    public class ImpactPlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public ImpactPlugin(IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory,
            WidgetSettings widgetSettings)
        {
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            var context = _actionContextAccessor.ActionContext;

            return context == null ? string.Empty : _urlHelperFactory.GetUrlHelper(context).RouteUrl(ImpactDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.HeadHtmlTag,
                PublicWidgetZones.CheckoutConfirmBottom,
                PublicWidgetZones.OpCheckoutConfirmBottom
            });
        }

        /// <summary>
        /// Gets a type of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component type</returns>
        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone is null)
                throw new ArgumentNullException(nameof(widgetZone));

            if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag))
                return typeof(WidgetsImpactHeadViewComponent);

            if (widgetZone.Equals(PublicWidgetZones.CheckoutConfirmBottom) || widgetZone.Equals(PublicWidgetZones.OpCheckoutConfirmBottom))
                return typeof(WidgetsImpactConfirmViewComponent);

            return null;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new ImpactSettings
            {
                RequestTimeout = ImpactDefaults.RequestTimeout
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(ImpactDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(ImpactDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.Impact.Configuration.Fields.Enabled"] = "Enabled",
                ["Plugins.Misc.Impact.Configuration.Fields.Enabled.Hint"] = "Determine whether the plugin is enabled.",
                ["Plugins.Misc.Impact.Configuration.Fields.AccountSId"] = "Account SID",
                ["Plugins.Misc.Impact.Configuration.Fields.AccountSId.Hint"] = "Enter the Account SID. You can find this in your Impact account > Settings > API.",
                ["Plugins.Misc.Impact.Configuration.Fields.AccountSId.Required"] = "Account SID is required",
                ["Plugins.Misc.Impact.Configuration.Fields.AuthToken"] = "Authorization Token",
                ["Plugins.Misc.Impact.Configuration.Fields.AuthToken.Hint"] = "Enter the Authorization Token. You can find this in your Impact account > Settings > API.",
                ["Plugins.Misc.Impact.Configuration.Fields.AuthToken.Required"] = "Authorization Token is required",
                ["Plugins.Misc.Impact.Configuration.Fields.ProgramId"] = "Program ID",
                ["Plugins.Misc.Impact.Configuration.Fields.ProgramId.Hint"] = "Enter the Program ID. You can find this in your Impact account. At the top left, click the Program Name > Programs",
                ["Plugins.Misc.Impact.Configuration.Fields.ProgramId.Required"] = "Program ID is required",
                ["Plugins.Misc.Impact.Configuration.Fields.ActionTrackerId"] = "Action Tracker ID",
                ["Plugins.Misc.Impact.Configuration.Fields.ActionTrackerId.Hint"] = "Enter the Action Tracker ID.  You can find this in your Impact account > Settings > Tracking > Event Types.",
                ["Plugins.Misc.Impact.Configuration.Fields.ActionTrackerId.Required"] = "Action Tracker ID is required",
                ["Plugins.Misc.Impact.Configuration.Fields.UniversalTrackingScript"] = "Universal Tracking Script",
                ["Plugins.Misc.Impact.Configuration.Fields.UniversalTrackingScript.Hint"] = "Enter the Universal Tracking Script.  You can find this in your Impact account > Settings > Tracking > General > Universal Tracking Tag field.",
                ["Plugins.Misc.Impact.Configuration.Fields.UniversalTrackingScript.Required"] = "Universal Tracking Script is required",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            if (_widgetSettings.ActiveWidgetSystemNames.Contains(ImpactDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(ImpactDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await _settingService.DeleteSettingAsync<ImpactSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.Impact");

            await base.UninstallAsync();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;

        #endregion
    }
}