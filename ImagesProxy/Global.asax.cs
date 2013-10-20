using System.Web.Http;

namespace ImagesProxy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}