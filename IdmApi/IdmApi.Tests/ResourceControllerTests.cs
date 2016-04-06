using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdmApi.Controllers;
using IdmNet;
using IdmNet.Fakes;
using IdmNet.Models;
using Xunit;

namespace IdmApi.Tests
{
    public class ResourceControllerTests
    {
        [Fact]
        public void Get_returns_the_expected_items()
        {
            var stub = new StubIIdmNetClient
            {
                SearchAsyncSearchCriteriaInt32 = (criteria, num) =>
                {
                    IEnumerable<IdmResource> list = new List<IdmResource>();
                    return Task.FromResult(list);
                }
            };

            var it = new ResourcesController {Client = stub};

            var result = it.Get("/Person");

        }
    }
}
