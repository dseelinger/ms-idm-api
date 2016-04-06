using System.Collections.Generic;
using System.Linq;
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

        [Route("api/resources/")]
        public async Task<IdmResource[]> Get(string filter)
        {
            SearchCriteria criteria = new SearchCriteria(filter);
            IEnumerable<IdmResource> result = await Client.SearchAsync(criteria);
            var ary = result.ToArray();
            
            return ary;
        }
    }
}
