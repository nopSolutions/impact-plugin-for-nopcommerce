using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Impact.Models
{
    /// <summary>
    /// Represents a configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the plugin is enabled
        /// </summary>
        [NopResourceDisplayName("Plugins.Misc.Impact.Configuration.Fields.Enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the account SID
        /// </summary>
        [NopResourceDisplayName("Plugins.Misc.Impact.Configuration.Fields.AccountSId")]
        public string AccountSId { get; set; }

        /// <summary>
        /// Gets or sets the authorization Token
        /// </summary>
        [NopResourceDisplayName("Plugins.Misc.Impact.Configuration.Fields.AuthToken")]
        [DataType(DataType.Password)]
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the program ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Misc.Impact.Configuration.Fields.ProgramId")]
        public string ProgramId { get; set; }

        /// <summary>
        /// Gets or sets the action tracker ID
        /// </summary>
        [NopResourceDisplayName("Plugins.Misc.Impact.Configuration.Fields.ActionTrackerId")]
        public string ActionTrackerId { get; set; }

        /// <summary>
        /// Gets or sets the universal tracking script
        /// </summary>
        [NopResourceDisplayName("Plugins.Misc.Impact.Configuration.Fields.UniversalTrackingScript")]
        public string UniversalTrackingScript { get; set; }
    }
}