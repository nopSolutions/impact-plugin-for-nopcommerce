using FluentValidation;
using Nop.Plugin.Misc.Impact.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Misc.Impact.Validators
{
    /// <summary>
    /// Represents configuration model validator
    /// </summary>
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.AccountSId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Impact.Configuration.Fields.AccountSId.Required"))
                .When(model => model.Enabled);

            RuleFor(model => model.ProgramId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Impact.Configuration.Fields.ProgramId.Required"))
                .When(model => model.Enabled);

            RuleFor(model => model.ActionTrackerId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Impact.Configuration.Fields.ActionTrackerId.Required"))
                .When(model => model.Enabled);

            RuleFor(model => model.AuthToken)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Impact.Configuration.Fields.AuthToken.Required"))
                .When(model => model.Enabled);

            RuleFor(model => model.UniversalTrackingScript)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Misc.Impact.Configuration.Fields.UniversalTrackingScript.Required"))
                .When(model => model.Enabled);
        }

        #endregion
    }
}