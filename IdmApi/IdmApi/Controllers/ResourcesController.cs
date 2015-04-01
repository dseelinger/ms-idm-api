using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using IdmApi.Models;
using IdmNet;

namespace IdmApi.Controllers
{
#pragma warning disable 1591
    public class ResourcesController : ApiController
#pragma warning restore 1591
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
        /// <param name="id">Resource ID/ObjectID/GUID that matches the Identity Manager object to retrieve</param>
        /// <param name="select">
        /// (optional) Comma separated list of attributes of the Identity Manager object to return. Defaults to only 
        /// ObjectId and ObjectType, which are always returned. Remember that if all attributes are not returned then
        /// some of the attributes may appear null when in fact they are populated inside the Identity Manager Service
        /// DB.
        /// </param>
        public async Task<IdmResource> GetById(string id, string select = null)
        {
            var attributes = (select == null) ? null : select.Split(',');
            return await _repo.GetById(id, attributes);
        }

        /// <summary>
        /// Get one or more resources from Identity Manager
        /// </summary>
        /// <param name="filter">XPath query filter to return specific Identity Manager objects. Defaults to "/*", 
        /// which returns all objects.</param>
        /// <param name="select">Comma separated list of attributes of the Identity Manager object to return.  
        /// Defaults to ObjectId and ObjectType, which are always returned.</param>
        public async Task<IEnumerable<IdmResource>> GetByFilter(string filter = "/*", string select = null)
        {
            var attributes = (select == null) ? null : select.Split(',');
            return await _repo.GetByFilter(filter, attributes);
        }
    }
}