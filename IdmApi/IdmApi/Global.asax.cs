using System.Web;
using System.Web.Http;

namespace IdmApi
{
    /// <summary>
    /// Web App
    /// </summary>
    public class WebApiApplication : HttpApplication
    {
        /// <summary>
        /// Startup method
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
