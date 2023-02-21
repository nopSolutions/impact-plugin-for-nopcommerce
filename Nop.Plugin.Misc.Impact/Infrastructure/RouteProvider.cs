using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.Impact.Infrastructure
{
    /// <summary>
    /// Represents plugin route provider
    /// </summary>
    public class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(name: ImpactDefaults.ConfigurationRouteName,
                pattern: "Admin/Impact/Configure",
                defaults: new { controller = "ImpactAdmin", action = "Configure", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: ImpactDefaults.SetClickIdRouteName,
                pattern: "Impact/SetClickId",
                defaults: new { controller = "ImpactPublic", action = "SetClickId" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}