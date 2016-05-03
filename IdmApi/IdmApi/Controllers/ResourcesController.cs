using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using IdmNet;
using IdmNet.Models;
using IdmNet.SoapModels;

namespace IdmApi.Controllers
{
    public class ResourcesController : ApiController
    {
        public IIdmNetClient Client { get; set; }

        public ResourcesController()
        {
            Client = IdmNetClientFactory.BuildClient();
        }

        /// <summary>
        /// Get one or more resources from Identity Manager
        /// </summary>
        /// <remarks>
        /// Passing doPagedSearch=true only returns partial result, if the pageSize is smaller than the number of 
        /// records found in the search and a header called x-idm-next-link will contain a link that may be used to 
        /// return the next page of results and so on, until all records are retieved. When all records have been 
        /// retrieved, no x-idm-next-link will be present in the response.
        /// </remarks>
        /// <param name="filter">XPath query filter to return specific Identity Manager objects.</param>
        /// <param name="select">
        ///     Comma separated list of attributes of the Identity Manager object to return.  
        ///     Defaults to ObjectId and ObjectType, which are always returned.
        /// </param>
        /// <param name="sort">
        /// Comma separated list of attributes to sort by, must be in the format of  "AttributeName:SortDirection"
        /// For example: BoundObjectType:Ascending,BoundAttributeType:Descending - which would be a valid sort order 
        /// for BindingDescription objects in Identity Manager
        /// </param>
        /// <param name="pageSize">
        /// The number of records to return from Identity Manager at a time.  Note that passing this a value and not 
        /// passing doPagedSearc = true will cause the back end to bring back each page of results behind the scenes 
        /// and still return all the records at the end of those back-end retrievals.
        /// </param>
        /// <param name="doPagedSearch">
        /// If true, then up to "pageSize" records will be returned, and the next-link will be returned in the 
        /// x-idm-next-link header if more records are available.
        /// </param>
        /// <response code="200">OK</response>
        [Route("api/resources/")]
        public async Task<IdmResource[]> Get(string filter, string @select = null, string sort = null, int pageSize = 50, bool doPagedSearch = false)
        {
            var attributes = @select?.Split(',').ToList();
            var searchCriteria = new SearchCriteria(filter) { Selection = attributes };
            ParseEachSort(sort, searchCriteria);


            IEnumerable<IdmResource> result = await Client.SearchAsync(searchCriteria);
            var ary = result.ToArray();
            
            return ary;
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
        /// <response code="200">OK</response>
        [Route("api/resources/{id}")]
        public async Task<IdmResource> GetById(string id, string @select = null)
        {
            List<string> attributes = new List<string>();
            if (@select != null) attributes = RemoveAllWhitespace(@select).Split(',').ToList();
            IdmResource result = await Client.GetAsync(id, attributes);

            return result;
        }

        /// <summary>
        /// Get the count of the number of records that match (or would be returned) from a particular search request
        /// </summary>
        /// <remarks>
        /// HEAD requests by definition don't return anything in the body of the response.  Instead, the count will 
        /// appear in a custom header called "x-idm-count"
        /// </remarks>
        /// <param name="filter">XPath query filter to perform the search.</param>
        /// <response code="204">No Content</response>
        [Route("api/resources/")]
        public async Task<HttpResponseMessage> Head(string filter)
        {
            int count = await Client.GetCountAsync(filter);

            var response = Request.CreateResponse(HttpStatusCode.NoContent);
            response.Headers.Add("x-idm-count", new[] { count.ToString() });

            return response;
        }

        /// <summary>
        /// POST /api/resouces/ Create a new Resource object in Identity Manager
        /// </summary>
        /// <param name="resource">New Identity Manager resource</param>
        /// <remarks>HTTP Response 201 (Created) with Location Header and resulting resource with its ObjectID populated.</remarks>
        /// <response code="201">Created</response>
        [Route("api/resources/")]
        public async Task<HttpResponseMessage> Post(IdmResource resource)
        {
            Message soapResponse = await Client.CreateAsync(resource);
            resource.ObjectID = Client.GetNewObjectId(soapResponse);

            HttpResponseMessage apiResponse = Request.CreateResponse(HttpStatusCode.Created, resource);
            apiResponse.Headers.Location =
                new Uri(Request.RequestUri.OriginalString + "/" + resource.ObjectID);

            return apiResponse;
        }


        private static void ParseEachSort(string sort, SearchCriteria searchCriteria)
        {
            if (sort != null)
            {
                var sortAttributes = new List<SortingAttribute>();
                foreach (var sortDefinition in sort.Split(','))
                {
                    ParseSort(sortDefinition, sortAttributes);
                }
                searchCriteria.Sorting.SortingAttributes = sortAttributes.ToArray();
            }
        }

        private static void ParseSort(string sortDefinition, List<SortingAttribute> sortAttributes)
        {
            var sortParts = sortDefinition.Split(':');
            if (sortParts.Length != 2)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = "BAD REQUEST",
                    Content =
                        new StringContent(
                            "sort must be a comma separated list of attributes to sort by, must be in the format of  'AttributeName:SortDirection'. For example: BoundObjectType:Ascending,BoundAttributeType:Descending - which would be a valid sort order for BindingDescription objects in Identity Manager")
                });
            }
            sortAttributes.Add(new SortingAttribute
            {
                Ascending = (sortParts[1].ToLower() != "descending"),
                AttributeName = sortParts[0]
            });
        }

        private static string RemoveAllWhitespace(string @select)
        {
            return Regex.Replace(@select, @"\s+", "");
        }

    }
}