using System.Web.Http;

namespace ImagesProxy
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "Retrieve",
                routeTemplate: "{height}/{width}/{source}",
                defaults: new { controller = "Image", action = "Retrieve" }
            );
        }
    }
}
