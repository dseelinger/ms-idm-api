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
using IdmNet.SoapModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdmApi.Tests
{
    [TestClass]
    public class ResourceControllerTests
    {
        [TestMethod]
        public async Task T001_It_can_search_for_all_ObjectTypeDescription_resources_without_specifying_select_or_sort()
        {
            var filter = "/ObjectTypeDescription";
            var resources = new List<IdmResource>
            {
                new IdmResource(),
                new IdmResource()
            };

            var repo = new StubIRepository
            {
                GetByFilterSearchCriteria = criteria =>
                {
                    Assert.AreEqual(1, criteria.Sorting.SortingAttributes.Count());
                    Assert.IsTrue(criteria.Sorting.SortingAttributes[0].Ascending);
                    Assert.AreEqual("DisplayName", criteria.Sorting.SortingAttributes[0].AttributeName);
                    Assert.AreEqual(2, criteria.Selection.Count);
                    Assert.AreEqual("ObjectID", criteria.Selection[0]);
                    Assert.AreEqual("ObjectType", criteria.Selection[1]);
                    Assert.AreEqual(filter, criteria.Filter.Query);
                    return Task.FromResult((IEnumerable<IdmResource>) resources);
                }
            };

            var it = new ResourcesController(repo);

            var result = await it.GetByFilter(filter);

            Assert.AreEqual(2, result.Count());

        }

        // TODO: Sloppy Select
        // TODO: Sloppy Filter
        // TODO: throw on null filter
        // TODO: Sorting

        // TODO: Head

        [TestMethod]
        public async Task It_can_add_a_value_to_a_multi_valued_attribute_that_already_has_one_or_more_values
            ()
        {
            var objectId = Guid.NewGuid().ToString("D");
            var attrName = "ProxyAddressesCollection";
            var newValue = "joecool@snoopy.com";

            // Arrange
            var repo = new StubIRepository
            {
                PostAttributeStringStringString = (objId, name, val) =>
                {
                    Assert.AreEqual(objectId, objId);
                    Assert.AreEqual(attrName, name);
                    Assert.AreEqual(newValue, val);

                    var msg = Message.CreateMessage(MessageVersion.Default, "Doesn't matter");
                    return Task.FromResult(msg);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            // Act
            HttpResponseMessage result = await it.PostAttribute(objectId, attrName, newValue);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual(string.Format("http://myserver/api/resources/{0}/{1}", objectId, attrName), result.Headers.Location.ToString());
        }



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
            var idmResource = new IdmResource { DisplayName = "foo" };
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
        public async Task It_can_Post_a_resource()
        {
            var resource = new IdmResource { DisplayName = "foo" };

            var repo = new StubIRepository
            {
                PostIdmResource = idmResource =>
                {
                    idmResource.ObjectID = "bar";
                    return Task.FromResult(idmResource);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
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

        [TestMethod]
        public async Task It_can_PostAttribute()
        {
            // Arrange
            var repo = new StubIRepository
            {
                PostAttributeStringStringString = (objId, name, val) =>
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

            // Act
            HttpResponseMessage result = await it.PostAttribute("foo", "bar", "bat");

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
            Assert.AreEqual("http://myserver/api/resources/foo/bar", result.Headers.Location.ToString());
        }

        [TestMethod]
        public async Task It_can_DeleteAttribute()
        {
            // Arrange
            var repo = new StubIRepository
            {
                DeleteAttributeStringStringString = (objId, name, val) =>
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

            // Act
            HttpResponseMessage result = await it.DeleteAttribute("foo", "bar", "bat");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [TestMethod]
        public async Task It_can_PutChanges()
        {
            var changes1 = new[]
            {
                new Change(ModeType.Replace, "FirstName", "FirstNameTest"),
                new Change(ModeType.Replace, "LastName", "LastNameTest"),
                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab1.lab"),
                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab2.lab"),
            };

            var repo = new StubIRepository
            {
                PutChangesStringChangeArray = (objId, changes) =>
                {
                    Assert.AreEqual("foo", objId);
                    Assert.AreEqual(changes1, changes);

                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
                    return Task.FromResult(msg);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            HttpResponseMessage result = await it.PutChanges("foo", changes1);

            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [TestMethod]
        public async Task It_can_DeleteResource()
        {
            // Arrange
            var repo = new StubIRepository
            {
                DeleteResourceString = (objId) =>
                {
                    Assert.AreEqual("id", objId);

                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
                    return Task.FromResult(msg);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            // Act
            HttpResponseMessage result = await it.DeleteResource("id");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }
    }
}

// TODO 012: Implement the Resource client Get operation (as opposed to Enumerate+Pull)
// TODO 011: Get Count
// TODO 010: Implement GetSchema(string objectTypeName)
// TODO 009: Implement Select *
// TODO 008: Implement Paging
// TODO 007: Implement /api/persons
// TODO 006: Implement /api/groups
// TODO 005: Implement /api/attributetypedescriptions
// TODO 004: Implement /api/objecttypedescriptions
// TODO 003: Implement /api/bindingdescriptions
// TODO 002: Implement /api/whatever
// TODO 001: Implement Approvals
// TODO -999: Implement the STS endpoint
