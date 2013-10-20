using System.Web.Mvc;
using System.Web.Routing;

namespace ImagesProxy
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "i/{height}/{width}/{source}",
                defaults: new { controller = "Image", action = "Retrieve"}
            );
        }
    }
}