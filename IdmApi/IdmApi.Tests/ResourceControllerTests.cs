using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
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

        [TestMethod]
        public async Task It_can_GetAttributeById_for_null_attributes()
        {
            var idmResource = new IdmResource
            {
                Attributes =
                    new List<IdmAttribute>()
            };
            var repo = new StubIRepository
            {
                GetByIdStringStringArray = (s, strings) => Task.FromResult(idmResource)
            };

            var it = new ResourcesController(repo);

            var result = (JObject)await it.GetAttributeById("foo", "bar");

            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task It_can_GetByFilter()
        {
            var resources = new List<IdmResource>
            {
                new IdmResource(),
                new IdmResource()
            };
            IEnumerable<IdmResource> res = resources;

            var repo = new StubIRepository
            {
                GetByFilterStringStringArray = (s, strings) => Task.FromResult(res)
            };

            var it = new ResourcesController(repo);

            var result = await it.GetByFilter("foo", "bar");

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task It_can_Post_a_resource()
        {
            var resource = new IdmResource {DisplayName = "foo"};

            var repo = new StubIRepository
            {
                PostIdmResource = idmResource =>
                {
                    idmResource.ObjectID = "bar";
                    return Task.FromResult(idmResource);
                }
            };

            var it = new ResourcesController(repo) {Request = new HttpRequestMessage()};
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            HttpResponseMessage result = await it.Post(resource);

            var json = await result.Content.ReadAsStringAsync();
            var resourceResult = JsonConvert.DeserializeObject<IdmResource>(json);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual("bar", resourceResult.ObjectID);
            Assert.AreEqual("http://myserver/api/resources/bar", result.Headers.Location.ToString());
        }

        [TestMethod]
        public async Task It_can_PutAttribute()
        {
            //[TestMethod]
            //[TestCategory("Integration")]
            //public async Task It_can_PutAttributeValueAsync_to_a_present_single_valued_attribute()
            //{
            //    await AssertReplaceOk("FirstName", "TestFirstName1", "TestFirstName2");
            //}

            //var it = IdmNetClientFactory.BuildClient();
            //IdmResource testResource = await CreateTestPerson(it);

            //try
            //{
            //    // Act
            //    await it.ReplaceValueAsync(testResource.ObjectID, attrName, attrValue1);
            //    await it.ReplaceValueAsync(testResource.ObjectID, attrName, attrValue2);

            //    // Assert
            //    var searchResult =
            //        await
            //            it.SearchAsync(new SearchCriteria
            //            {
            //                XPath = "/Person[ObjectID='" + testResource.ObjectID + "']",
            //                Attributes = new[] { attrName }
            //            });
            //    Assert.AreEqual(attrValue2, searchResult.First().GetAttrValue(attrName));
            //}
            //finally
            //{
            //    // Afterwards
            //    it.DeleteAsync(testResource.ObjectID);
            //}



            var repo = new StubIRepository
            {
                PutAttributeStringStringString = (objId, name, val) =>
                {
                    Assert.AreEqual("foo", objId);
                    Assert.AreEqual("bar", name);
                    Assert.AreEqual("bat", val);

                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
                    return Task.FromResult(msg);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");


            HttpResponseMessage result = await it.PutAttribute("foo", "bar", "bat");

            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);

        }


        // TODO 004: Put Attribute (Replace)
        // TODO 003: Post Attribute (Add)
        // TODO 002: Delete Attribute (Delete)
        // TODO 001: Put Changes (batch update)
        // TODO 000: Delete Resource

    }
}
