using System.Collections.Generic;
using System.Threading.Tasks;
using IdmApi.Controllers;
using IdmApi.DAL.Fakes;
using IdmNet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdmApi.Tests
{
    [TestClass]
    public class ResourceControllerTests
    {
        [TestMethod]
        public void It_has_a_CTOR_that_takes_a_repo()
        {
            var repo = new StubIRepository();

            var it = new ResourcesController(repo);

            Assert.AreEqual(repo, it.Repo);
        }

        [TestMethod]
        public async Task It_can_GetById_with_no_select()
        {
            var idmResource = new IdmResource{DisplayName = "foo"};
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) => Task.FromResult(idmResource)
            };

            var it = new ResourcesController(repo);

            var result = await it.GetById("foo");

            Assert.AreEqual(idmResource, result);
        }

        [TestMethod]
        public async Task It_can_GetById_with_select()
        {
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) =>
                {
                    Assert.AreEqual("bar", strings[0]);
                    Assert.AreEqual("bat", strings[1]);
                    return Task.FromResult(new IdmResource());
                }
            };

            var it = new ResourcesController(repo);

            await it.GetById("foo", "bar,bat");
        }

        [TestMethod]
        public async Task It_can_GetById_with_sloppy_select()
        {
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) =>
                {
                    Assert.AreEqual("bar", strings[0]);
                    Assert.AreEqual("bat", strings[1]);
                    return Task.FromResult(new IdmResource());
                }
            };

            var it = new ResourcesController(repo);

            await it.GetById("foo", " bar, bat ");
        }

        [TestMethod]
        public async Task It_can_GetAttributeById()
        {
            var idmResource = new IdmResource
            {
                Attributes = new List<IdmAttribute> { new IdmAttribute { Name = "bar", Value = "bat" } }
            };
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) => Task.FromResult(idmResource)
            };

            var it = new ResourcesController(repo);

            var result = (JObject)await it.GetAttributeById("foo", "bar");

            Assert.AreEqual("bat", result["bar"]);
        }

        [TestMethod]
        public async Task It_can_GetAttributeById_for_multi_valued_atttributes()
        {
            var idmResource = new IdmResource
            {
                Attributes =
                    new List<IdmAttribute> { new IdmAttribute { Name = "bar", Values = new List<string> { "fiz", "buz" } } }
            };
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) => Task.FromResult(idmResource)
            };

            var it = new ResourcesController(repo);

            var result = (JObject)await it.GetAttributeById("foo", "bar");

            Assert.AreEqual("fiz", result["bar"][0]);
            Assert.AreEqual("buz", result["bar"][1]);
        }

        //[TestMethod]
        //public async Task It_can_GetAttributeById_for_null_attributes()
        //{
        //    var idmResource = new IdmResource
        //    {
        //        Attributes =
        //            new List<IdmAttribute> { new IdmAttribute { Name = "bar" } }
        //    };
        //    var repo = new StubIRepository
        //    {
        //        GetByIdStringStringArray = (s, strings) => Task.FromResult(idmResource)
        //    };

        //    var it = new ResourcesController(repo);

        //    var result = (JObject)await it.GetAttributeById("foo", "bar");

        //    Assert.IsNull(result["bar"]);
        //}


    }
}
