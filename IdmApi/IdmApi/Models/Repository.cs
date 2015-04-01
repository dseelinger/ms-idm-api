#pragma warning disable 1591
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdmNet;

namespace IdmApi.Models
{
    public class Repository : IRepository
    {
        private readonly IdmNetClient _idmNet;

        public Repository(IdmNetClient idmNet)
        {
            _idmNet = idmNet;
        }

        public async Task<IdmResource> GetById(string id, string[] attributes)
        {
            var criteria = new SearchCriteria { Attributes = attributes, XPath = "/*[ObjectID='" + id + "']" };
            var searchResults = await _idmNet.SearchAsync(criteria);
            return searchResults.FirstOrDefault();
        }

        public async Task<IEnumerable<IdmResource>> GetByFilter(string filter, string[] attributes)
        {
            var criteria = new SearchCriteria { Attributes = attributes, XPath = filter };
            var searchResults = await _idmNet.SearchAsync(criteria);
            return searchResults;
        }
    }
}