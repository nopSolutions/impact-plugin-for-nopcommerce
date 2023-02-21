using Nop.Core;

namespace Nop.Plugin.Misc.Impact
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class ImpactDefaults
    {
        /// <summary>
        /// Gets the system name of plugin
        /// </summary>
        public static string SystemName => "Misc.Impact";

        /// <summary>
        /// Gets the user agent used to request third-party services
        /// </summary>
        public static string UserAgent => $"nopCommerce-{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Misc.Impact.Configure";

        /// <summary>
        /// Gets the SetClickId route name
        /// </summary>
        public static string SetClickIdRouteName => "Plugin.Misc.Impact.SetClickId";

        /// <summary>
        /// Gets the ClickId attribute name
        /// </summary>
        public static string ClickIdAttributeName => "Impact.ClickId";

        /// <summary>
        /// Gets the ClickId query parameter name
        /// </summary>
        public static string ClickIdQueryParamName => "irclickid";

        /// <summary>
        /// Gets the ClickId cookie prefix
        /// </summary>
        public static string ClickIdQueryCookiePrefix => "IR_";

        /// <summary>
        /// Gets the base API URL
        /// </summary>
        public static string ApiUrl => "https://api.impact.com/Advertisers/";

        /// <summary>
        /// Gets the name of the hash algorithm
        /// </summary>
        public static string HashAlgorithm => "SHA1";

        /// <summary>
        /// Gets a default period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 10;
    }
}