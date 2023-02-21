using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Impact
{
    /// <summary>
    /// Represents plugin settings
    /// </summary>
    public class ImpactSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the plugin is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the account SID
        /// </summary>
        public string AccountSId { get; set; }

        /// <summary>
        /// Gets or sets the authorization Token
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the program ID
        /// </summary>
        public string ProgramId { get; set; }

        /// <summary>
        /// Gets or sets the action tracker ID
        /// </summary>
        public string ActionTrackerId { get; set; }

        /// <summary>
        /// Gets or sets the universal tracking script
        /// </summary>
        public string UniversalTrackingScript { get; set; }

        #region Advanced

        /// <summary>
        /// Gets or sets a period (in seconds) before the request times out
        /// </summary>
        public int? RequestTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to log request/response details
        /// </summary>
        public bool LogRequests { get; set; }

        #endregion
    }
}