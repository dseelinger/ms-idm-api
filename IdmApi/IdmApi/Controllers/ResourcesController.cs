using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using IdmApi.DAL;
using IdmNet.Models;
using Newtonsoft.Json.Linq;

namespace IdmApi.Controllers
{
    /// <summary>
    /// Resource Controller
    /// </summary>
    public class ResourcesController : ApiController
    {
        /// <summary>
        /// Repository
        /// </summary>
        public IRepository Repo { get; set; }

        /// <summary>
        /// Resources Controller constructor
        /// </summary>
        /// <param name="repo"></param>
        public ResourcesController(IRepository repo)
        {
            Repo = repo;
        }


        /// <summary>
        /// Get a resource by its ID
        /// </summary>
        /// <param name="id">ObjectID that matches the Identity Manager object to retrieve</param>
        /// <param name="select">
        /// (optional) Comma separated list of attributes of the Identity Manager object to return. Defaults to only 
        /// ObjectId and ObjectType, which are always returned. Remember that if all attributes are not returned then
        /// some of the attributes may appear null when in fact they are populated inside the Identity Manager Service
        /// DB.
        /// </param>
        [Route("api/resources/{id}")]
        public async Task<IdmResource> GetById(string id, string @select = null)
        {
            string[] attributes = null;
            if (@select != null)
                attributes = RemoveAllWhitespace(@select).Split(',');
            return await Repo.GetById(id, attributes);
        }

        /// <summary>
        /// Get a resource by its ID
        /// </summary>
        /// <param name="id">ObjectID that matches the Identity Manager object to retrieve</param>
        /// <param name="attribute">Attribute for the resource to return</param>
        [Route("api/resources/{id}/{attribute}")]
        public async Task<object> GetAttributeById(string id, string attribute)
        {

            var resource = await Repo.GetById(id, new[] { attribute });

            var attr = resource.GetAttr(attribute);
            if (attr != null)
            {
                if (attr.Values.Count > 1)
                {
                    return new JObject(new JProperty(attribute, attr.Values));
                }
                return new JObject(new JProperty(attribute, attr.Value));
            }
            return null;
        }

        /// <summary>
        /// Get one or more resources from Identity Manager
        /// </summary>
        /// <param name="filter">XPath query filter to return specific Identity Manager objects. Defaults to "/*", 
        /// which returns all objects.</param>
        /// <param name="select">Comma separated list of attributes of the Identity Manager object to return.  
        /// Defaults to ObjectId and ObjectType, which are always returned.</param>
        [Route("api/resources/")]
        public async Task<IEnumerable<IdmResource>> GetByFilter(string filter, string select = null)
        {
            var attributes = (select == null) ? null : select.Split(',');
            return await Repo.GetByFilter(filter, attributes);
        }


        private static string RemoveAllWhitespace(string @select)
        {
            return Regex.Replace(@select, @"\s+", "");
        }


        /// <summary>
        /// Create a new Resource object in Identity Manager
        /// </summary>
        /// <param name="resource">New Identity Manager resource</param>
        /// <returns>HTTP Response 204 with Location Header and resulting resource with its ObjectID populated.</returns>
        public async Task<HttpResponseMessage> Post(IdmResource resource)
        {
            var resourceResult = await Repo.Post(resource);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, resourceResult);
            response.Headers.Location = new Uri(Request.RequestUri.OriginalString + "/api/resources/" + resourceResult.ObjectID);

            return response;
        }
    }
}