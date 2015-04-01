using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using IdmApi.Models;
using IdmNet;

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
        /// <param name="id">Resource ID/ObjectID/GUID that matches the Identity Manager object to retrieve</param>
        /// <param name="select">
        /// (optional) Comma separated list of attributes of the Identity Manager object to return. Defaults to only 
        /// ObjectId and ObjectType, which are always returned. Remember that if all attributes are not returned then
        /// some of the attributes may appear null when in fact they are populated inside the Identity Manager Service
        /// DB.
        /// </param>
        public async Task<IdmResource> Get(string id, string select = null)
        {
            var attributes = (select == null) ? null : select.Split(',');
            return await _repo.GetById(id, attributes);
        }
    }
}
