using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;

namespace IdmApi
{
    /// <summary>
    /// Web API dependency resolver for Unity
    /// </summary>
    public class UnityResolver : IDependencyResolver
    {
        /// <summary>
        /// Unity Container
        /// </summary>
        protected IUnityContainer Container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">Unity Container</param>
        public UnityResolver(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }
            Container = container;
        }

        /// <summary>
        /// Gets the item for the type in question
        /// </summary>
        /// <param name="serviceType">type to resolve</param>
        /// <returns>single object matching type</returns>
        public object GetService(Type serviceType)
        {
            try
            {
                return Container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets all objects that match the given type
        /// </summary>
        /// <param name="serviceType">type to resolve</param>
        /// <returns>multiple types matching object</returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return Container.ResolveAll(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return new List<object>();
            }
        }

        /// <summary>
        /// Starts a new scope for the resolver
        /// </summary>
        /// <returns>New resolver</returns>
        public IDependencyScope BeginScope()
        {
            var child = Container.CreateChildContainer();
            return new UnityResolver(child);
        }

        /// <summary>
        /// Disposes internal resources
        /// </summary>
        public void Dispose()
        {
            Container.Dispose();
        }
    }
}