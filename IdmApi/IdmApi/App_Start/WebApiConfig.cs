using System;
using System.Net;
using System.ServiceModel;
using System.Web.Http;
using IdmApi.Models;
using IdmNet;
using Microsoft.Practices.Unity;

namespace IdmApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var soapBinding = new IdmSoapBinding();
            var endpointAddress = new EndpointAddress(GetEnv("MIM_Enumeration_endpoint"));
            var credential = new NetworkCredential(
                GetEnv("MIM_username"),
                GetEnv("MIM_pwd"),
                GetEnv("MIM_domain"));
            var searchClient = new SearchClient(soapBinding, endpointAddress);
            if (searchClient.ClientCredentials != null)
                searchClient.ClientCredentials.Windows.ClientCredential = credential;
            else
                throw new ApplicationException("Could not construct Idm Search Client");
            var idmNet = new IdmNetClient(searchClient);

            var container = new UnityContainer();
            container.RegisterInstance(typeof(IRepository), new Repository(idmNet));

            config.DependencyResolver = new UnityResolver(container);


            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        private static string GetEnv(string environmentVariableName)
        {
            var environmentVariable = Environment.GetEnvironmentVariable(environmentVariableName);
            if (environmentVariable == null)
            {
                throw new ApplicationException("Missing Environment Variable: " + environmentVariableName);
            }
            return environmentVariable;
        }

    }
}
