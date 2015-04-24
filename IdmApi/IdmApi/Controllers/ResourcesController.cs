using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using IdmApi.DAL;
using IdmNet.Models;
using IdmNet.SoapModels;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedVariable

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
            return await Repo.GetByFilter(new SearchCriteria{Filter = new Filter(filter)});
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
        /// Get an attribute by its resource ID
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
        /// POST /api/resouces/ Create a new Resource object in Identity Manager
        /// </summary>
        /// <param name="resource">New Identity Manager resource</param>
        /// <returns>HTTP Response 201 (Created) with Location Header and resulting resource with its ObjectID populated.</returns>
        [Route("api/resources/")]
        public async Task<HttpResponseMessage> Post(IdmResource resource)
        {
            var resourceResult = await Repo.Post(resource);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, resourceResult);
            response.Headers.Location = new Uri(Request.RequestUri.OriginalString + "/api/resources/" + resourceResult.ObjectID);

            return response;
        }

        /// <summary>
        /// "PUT /api/resources/{ObjectID}/{attribute}" Set a single-valued attribute's value in Identity Manager 
        /// for a particular object.
        /// </summary>
        /// <remarks>
        /// This only works with Single-Valued attributes in Identity Manager.  For Multi-valued attributes use
        /// "POST /api/resources/{ObjectID}/{attribute}" to add a new (or initial) value to the multi-valued attribute 
        /// or use "DELETE /api/resources/{ObjectID}/{attribute}" to remove an existing attribute from the multi-valued
        /// attribute.
        /// </remarks>
        /// <param name="id">ObjectID of resource to modify</param>
        /// <param name="attribute">Name of the single-valued attribute to modify</param>
        /// <param name="attributeValue">New attribute value</param>
        /// <returns>204 (No Content) on success</returns>
        [Route("api/resources/{id}/{attribute}")]
        public async Task<HttpResponseMessage> PutAttribute(string id, string attribute, string attributeValue)
        {
            var result = await Repo.PutAttribute(id, attribute, attributeValue);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent);

            return response;
        }

        /// <summary>
        /// "POST /api/resources/{ObjectID}/{attribute}" - adds a new (or initial) value to a multi-valued attribute 
        /// for a particular object.
        /// </summary>
        /// <remarks>
        /// This only works with Multi-Valued attributes in Identity Manager. To modify a single-valued attribute, use
        /// "PUT /api/resources/{ObjectID}/{attribute}". Use "DELETE /api/resources/{ObjectID}/{attribute}" to remove 
        /// an existing attribute from the multi-valued attribute.
        /// </remarks>
        /// <param name="id">ObjectID of resource to be modified</param>
        /// <param name="attribute">Name of the multi-valued attribute to which to add a value</param>
        /// <param name="attributeValue">Value to add</param>
        /// <returns>
        /// HTTP Response 201 (Created) with Location of the attribute (returns all attribute values for the multi-valued 
        /// attribute.  No other resource or attribute data is returned with the response.
        /// </returns>
        [Route("api/resources/{id}/{attribute}")]
        public async Task<HttpResponseMessage> PostAttribute(string id, string attribute, string attributeValue)
        {
            var result = await Repo.PostAttribute(id, attribute, attributeValue);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, result);
            response.Headers.Location = new Uri(string.Format("{0}/api/resources/{1}/{2}", Request.RequestUri.OriginalString, id, attribute));

            return response;
        }

        /// <summary>
        /// "DELETE /api/resources/{ObjectID}/{attribute}" - deletes an existing value from a multi-valued attribute 
        /// for a particular object.
        /// </summary>
        /// <remarks>
        /// This only works with Multi-Valued attributes in Identity Manager. To remove the value of a single-valued 
        /// attribute, use "PUT /api/resources/{ObjectID}/{attribute}" with an empty string "" for the value. Use 
        /// "POST /api/resources/{ObjectID}/{attribute}" to a new or initial attribute to a multi-valued attribute.
        /// </remarks>
        /// <param name="id">ObjectID of resource to be modified</param>
        /// <param name="attribute">Name of the multi-valued attribute from which to remove a value</param>
        /// <param name="attributeValue">Value to remove</param>
        /// <returns> HTTP Response 204 (No Content) on success </returns>
        [Route("api/resources/{id}/{attribute}")]
        public async Task<HttpResponseMessage> DeleteAttribute(string id, string attribute, string attributeValue)
        {
            var result = await Repo.DeleteAttribute(id, attribute, attributeValue);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent);

            return response;
        }

        /// <summary>
        /// Make several attribute changes to an existing resource in Identity Manager
        /// </summary>
        /// <remarks>
        /// The array of changes is an array of JSON objects describing the changes to be made, where 
        /// Operation:0 = Single-value attribute REPLACE, Operation:1 = Multi-valued attribute ADD,
        /// and Operation:2 = Multi-valued attribute DELETE.  Following is an example of setting first name and last
        /// names to certain values and adding one multi-valued attribute to ProxyAddressCollection and removing 
        /// another: 
        /// [{"Operation":0,"AttributeName":"FirstName","AttributeValue":{"AttributeValue":{"FirstName":"FirstNameTest"}}},{"Operation":0,"AttributeName":"LastName","AttributeValue":{"AttributeValue":{"LastName":"LastNameTest"}}},{"Operation":1,"AttributeName":"ProxyAddressCollection","AttributeValue":{"AttributeValue":{"ProxyAddressCollection":"joe@lab1.lab"}}},{"Operation":2,"AttributeName":"ProxyAddressCollection","AttributeValue":{"AttributeValue":{"ProxyAddressCollection":"joe@lab2.lab"}}}]
        /// </remarks>
        /// <param name="id">Id</param>
        /// <param name="changes">Array of changes to be made</param>
        /// <returns> HTTP Response 204 (No Content) on success </returns>
        [Route("api/resources/{id}")]
        public async Task<HttpResponseMessage> PutChanges(string id, Change[] changes)
        {
            var result = await Repo.PutChanges(id, changes);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent);

            return response;
        }

        /// <summary>
        /// Delete a resource from Identity Manager
        /// </summary>
        /// <param name="id">ObjectID of the resource to be deleted</param>
        /// <returns> HTTP Response 204 (No Content) on success </returns>
        [Route("api/resources/{id}")]
        public async Task<HttpResponseMessage> DeleteResource(string id)
        {
            var result = await Repo.DeleteResource(id);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NoContent);

            return response;
        }

        private static string RemoveAllWhitespace(string @select)
        {
            return Regex.Replace(@select, @"\s+", "");
        }



    }
}