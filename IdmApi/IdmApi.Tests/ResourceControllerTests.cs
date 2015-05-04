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
        public async Task T001_It_can_search_for_specific_resources_without_specifying_select_or_sort()
        {
            var filter = "/ObjectTypeDescription";
            var resources = new List<IdmResource>
            {
                new IdmResource(),
                new IdmResource()
            };

            var repo = new StubIRepository
            {
                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    Assert.AreEqual(1, criteria.Sorting.SortingAttributes.Count());
                    Assert.IsTrue(criteria.Sorting.SortingAttributes[0].Ascending);
                    Assert.AreEqual("DisplayName", criteria.Sorting.SortingAttributes[0].AttributeName);
                    Assert.AreEqual(2, criteria.Selection.Count);
                    Assert.AreEqual(filter, criteria.Filter.Query);
                    return Task.FromResult((IEnumerable<IdmResource>)resources);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            var result = await it.GetByFilter(filter);
            var json = await result.Content.ReadAsStringAsync();
            var resourceResult = JsonConvert.DeserializeObject<IEnumerable<IdmResource>>(json);
            Assert.AreEqual(2, resourceResult.Count());

        }

        [TestMethod]
        public async Task T002_It_can_search_and_return_specific_attributes() 
        {
            const string filter = "/ObjectTypeDescription";
            var resources = new List<IdmResource>
            {
                new IdmResource(),
                new IdmResource()
            };

            var repo = new StubIRepository
            {
                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    Assert.AreEqual(4, criteria.Selection.Count);
                    Assert.AreEqual("DisplayName", criteria.Selection[2]);
                    Assert.AreEqual("Name", criteria.Selection[3]);
                    return Task.FromResult((IEnumerable<IdmResource>)resources);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            var result = await it.GetByFilter(filter, "DisplayName,Name");

            // Assert
            var json = await result.Content.ReadAsStringAsync();
            var resourceResult = JsonConvert.DeserializeObject<IEnumerable<IdmResource>>(json);
            Assert.AreEqual(2, resourceResult.Count());
        }

        [TestMethod]
        public async Task T003_It_can_search_and_return_all_attributes_with_Select_STAR()
        {
            var filter = "/ObjectTypeDescription";
            var resources = new List<IdmResource>
            {
                new IdmResource(),
                new IdmResource()
            };

            var repo = new StubIRepository
            {
                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    Assert.AreEqual(3, criteria.Selection.Count);
                    Assert.AreEqual("*", criteria.Selection[2]);
                    return Task.FromResult((IEnumerable<IdmResource>)resources);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            var result = await it.GetByFilter(filter, "*");

            // Assert
            var json = await result.Content.ReadAsStringAsync();
            var resourceResult = JsonConvert.DeserializeObject<IEnumerable<IdmResource>>(json);
            Assert.AreEqual(2, resourceResult.Count());
        }

        [TestMethod]
        public async Task T004_It_can_Search_and_Sort_the_results_by_multiple_attributes_in_Ascending_or_Descending_order()
        {
            var filter = "/BindingDescription";
            var resources = new List<IdmResource>
            {
                new IdmResource(),
                new IdmResource()
            };

            var repo = new StubIRepository
            {
                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    Assert.AreEqual(2, criteria.Sorting.SortingAttributes.Count());
                    Assert.IsTrue(criteria.Sorting.SortingAttributes[0].Ascending);
                    Assert.AreEqual("BoundObjectType", criteria.Sorting.SortingAttributes[0].AttributeName);
                    Assert.IsFalse(criteria.Sorting.SortingAttributes[1].Ascending);
                    Assert.AreEqual("BoundAttributeType", criteria.Sorting.SortingAttributes[1].AttributeName);

                    return Task.FromResult((IEnumerable<IdmResource>)resources);
                }
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            var result = await it.GetByFilter(filter, "*", "BoundObjectType:Ascending,BoundAttributeType:Descending");

            // Assert
            var json = await result.Content.ReadAsStringAsync();
            var resourceResult = JsonConvert.DeserializeObject<IEnumerable<IdmResource>>(json);
            Assert.AreEqual(2, resourceResult.Count());

        }

        [TestMethod]
        public async Task T005_It_can_get_a_resource_by_its_ObjectID()
        {
            // Arrange
            var idmResource = new IdmResource { DisplayName = "foo" };
            var repo = new StubIRepository
            {
                GetByIdStringListOfString = (id, @select) =>
                {
                    Assert.AreEqual("myID", id);
                    Assert.IsNull(@select);
                    return Task.FromResult(idmResource);
                }
            };
            var it = new ResourcesController(repo);

            // Act
            var result = await it.GetById("myID");

            // Assert
            Assert.AreEqual(idmResource, result);
        }


        [TestMethod]
        public async Task T006_It_can_get_any_or_all_attributes_for_a_resource_by_its_ObjectID()
        {
            // Arrange
            var idmResource = new IdmResource { DisplayName = "foo" };
            var repo = new StubIRepository
            {
                GetByIdStringListOfString = (id, @select) =>
                {
                    Assert.AreEqual("myID", id);
                    Assert.AreEqual("*", @select[0]);
                    return Task.FromResult(idmResource);
                }
            };
            var it = new ResourcesController(repo);

            // Act
            var result = await it.GetById("myID", "*");

            // Assert
            Assert.AreEqual(idmResource, result);
        }


        [TestMethod]
        public async Task T007_It_can_return_the_number_of_matching_records_for_a_given_search()
        {
            // Arrange
            var repo = new StubIRepository
            {
                GetCountString = filter =>
                {
                    Assert.AreEqual("/ConstantSpecifier", filter);
                    return Task.FromResult(97);
                }
            };
            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            // Act
            HttpResponseMessage result = await it.Head("/ConstantSpecifier");

            // Assert
            Assert.AreEqual("97", result.Headers.GetValues("x-idm-count").First());
        }

        [TestMethod]
        public async Task T008_It_can_create_objects_in_Identity_Manager()
        {
            var resource = new IdmResource { DisplayName = "foo" };

            var repo = new StubIRepository
            {
                CreateIdmResource = idmResource =>
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
        public async Task T009_It_can_delete_objects_from_Identity_Manager()
        {
            // Arrange
            var repo = new StubIRepository
            {
                DeleteResourceString = objId =>
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

        [TestMethod]
        public async Task T010_It_can_do_a_search_and_return_the_first_page_of_results_and_info_on_retrieving_subsequent_pages_if_any()
        {
            const string filter = "/ConstantSpecifier";
            PagedSearchResults pagedResults = new PagedSearchResults 
            {
                EndOfSequence = null,
                PagingContext =
                    new PagingContext
                    {
                        CurrentIndex = 25,
                        EnumerationDirection = "Forwards", 
                        Expires = "some time in the distant future",
                        Filter = "/ConstantSpecifier",
                        Selection = new[] {"DisplayName"},
                        Sorting = new Sorting()
                    },
                Items = new object(),
                Results = new List<IdmResource>
                {
                    new IdmResource(),
                    new IdmResource(),
                    new IdmResource()
                }
            };

            var repo = new StubIRepository 
            {
                //GetByFilterSearchCriteriaInt32Bool = (criteria, pageSize, doPagedSearch) =>
                //{
                //    Assert.AreEqual(1, criteria.Sorting.SortingAttributes.Count());
                //    Assert.IsTrue(criteria.Sorting.SortingAttributes[0].Ascending);
                //    Assert.AreEqual("DisplayName", criteria.Sorting.SortingAttributes[0].AttributeName);
                //    Assert.AreEqual(2, criteria.Selection.Count);
                //    Assert.AreEqual(filter, criteria.Filter.Query);
                //    return Task.FromResult((IEnumerable<IdmResource>)resources);
                //}
            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            var result = await it.GetByFilter(filter, pageSize: 33, doPagedSearch: true);

            //Assert.AreEqual(3, result.Count());



            //// Arrange
            //var it = IdmNetClientFactory.BuildClient();
            //var criteria = new SearchCriteria("/ObjectTypeDescription");
            //criteria.Selection.Add("DisplayName");

            //// Act
            //PagedSearchResults result = await it.GetPagedResultsAsync(criteria, 5);

            //// Assert
            //Assert.AreEqual("/ObjectTypeDescription", result.PagingContext.Filter);
            //Assert.AreEqual(5, result.PagingContext.CurrentIndex);
            //Assert.AreEqual("Forwards", result.PagingContext.EnumerationDirection);
            //Assert.AreEqual("9999-12-31T23:59:59.9999999", result.PagingContext.Expires);
            //Assert.AreEqual("ObjectID", result.PagingContext.Selection[0]);
            //Assert.AreEqual("ObjectType", result.PagingContext.Selection[1]);
            //Assert.AreEqual("DisplayName", result.PagingContext.Selection[2]);
            //Assert.AreEqual("DisplayName", result.PagingContext.Sorting.SortingAttributes[0].AttributeName);
            //Assert.AreEqual(true, result.PagingContext.Sorting.SortingAttributes[0].Ascending);

            //Assert.AreEqual("ObjectTypeDescription", result.Results[0].ObjectType);
            //Assert.AreEqual("Activity Information Configuration", result.Results[0].DisplayName);
            //Assert.AreEqual("Binding Description", result.Results[4].DisplayName);

        }


        // ETags endpoint
        // T011_It_can_get_resources_back_from_a_search_a_page_at_a_time
        // Should return 404 for "not found" stuff














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
                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
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
                GetByIdStringListOfString = (s, strings) =>
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
                GetByIdStringListOfString = (s, strings) => 
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
                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
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
                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
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
                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
            };

            var it = new ResourcesController(repo);

            var result = (JObject)await it.GetAttributeById("foo", "bar");

            Assert.IsNull(result);
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

    }
}

// TODO 007: Implement /api/persons
// TODO 006: Implement /api/groups
// TODO 005: Implement /api/attributetypedescriptions
// TODO 004: Implement /api/objecttypedescriptions
// TODO 003: Implement /api/bindingdescriptions
// TODO 002: Implement /api/whatever
// TODO 001: Implement Approvals
// TODO -999: Implement the STS endpoint
