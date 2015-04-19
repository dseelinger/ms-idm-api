using System;
using System.Web.Http;
using IdmApi.DAL;
using IdmNet;
using Microsoft.Practices.Unity;

namespace IdmApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var idmNet = IdmNetClientFactory.BuildClient();

            var container = new UnityContainer();
            container.RegisterInstance(typeof(IRepository), new Repository(idmNet));

            config.DependencyResolver = new UnityResolver(container);


            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
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
