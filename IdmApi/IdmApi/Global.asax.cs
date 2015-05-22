using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

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
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        }
    }
}
