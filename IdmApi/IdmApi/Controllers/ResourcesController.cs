﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using IdmApi.DAL;
using IdmNet.Models;
using Newtonsoft.Json.Linq;

namespace IdmApi.Controllers
{
    public class ResourcesController : ApiController
    {
        private readonly IRepository _repo;

        /// <summary>
        /// Resources Controller constructor
        /// </summary>
        /// <param name="repo"></param>
        public ResourcesController(IRepository repo)
        {
            _repo = repo;
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
            return await _repo.GetById(id, attributes);
        }

        /// <summary>
        /// Get a resource by its ID
        /// </summary>
        /// <param name="id">ObjectID that matches the Identity Manager object to retrieve</param>
        /// <param name="attribute">Attribute for the resource to return</param>
        [Route("api/resources/{id}/{attribute}")]
        public async Task<object> GetAttributeById(string id, string attribute)
        {

            var resource = await _repo.GetById(id, new[] { attribute });

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
            return await _repo.GetByFilter(filter, attributes);
        }


        private static string RemoveAllWhitespace(string @select)
        {
            return Regex.Replace(@select, @"\s+", "");
        }


    }
}