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
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
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
            // Arrange
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
                        Selection = new[] { "DisplayName" },
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
            var etagRes = new List<IdmResource>
            {
                new IdmResource()
            };

            var pagedResultsCallCount = 0;
            var filterCallCount = 0;
            var createCallCount = 0;
            var repo = new StubIRepository
            {
                GetPagedResultsSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    pagedResultsCallCount++;
                    Assert.AreEqual(33, pageSize);
                    return Task.FromResult(pagedResults);
                },
                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    filterCallCount++;
                    Assert.AreEqual(1, pageSize);
                    Assert.AreEqual("/ObjectTypeDescription[Name='ETag']", criteria.Filter.Query);
                    return Task.FromResult((IEnumerable<IdmResource>)etagRes);
                },
                CreateIdmResource = resource =>
                {
                    createCallCount++;
                    Assert.AreEqual("ETag", resource.ObjectType);
                    return Task.FromResult(new IdmResource { ObjectID = "foo" });
                }

            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            // Act
            var result = await it.GetByFilter(filter, pageSize: 33, doPagedSearch: true);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.AreEqual(1, pagedResultsCallCount);
            Assert.AreEqual(1, filterCallCount);
            Assert.AreEqual(1, createCallCount);
            Assert.AreEqual("http://myserver/api/etags/foo", result.Headers.GetValues("x-idm-next-link").FirstOrDefault());

            string json = await result.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<PagedSearchResults>(json);
            Assert.AreEqual(3, content.Results.Count);
        }

        // Test what happens when the ETag ObjectType doesn't exist
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


        [TestMethod]
        public async Task It_can_do_a_search_and_return_the_first_page_of_results_and_info_on_retrieving_subsequent_pages_even_if_ETag_doesnt_exist_in_FIM()
        {
            // Arrange
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
                        Selection = new[] { "DisplayName" },
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
            var etagRes = new List<IdmResource>{};

            var pagedResultsCallCount = 0;
            var filterCallCount = 0;
            var createCallCount = 0;
            var repo = new StubIRepository
            {
                GetPagedResultsSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    pagedResultsCallCount++;
                    Assert.AreEqual(33, pageSize);
                    return Task.FromResult(pagedResults);
                },
                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
                {
                    filterCallCount++;
                    Assert.AreEqual(1, pageSize);
                    Assert.AreEqual("/ObjectTypeDescription[Name='ETag']", criteria.Filter.Query);
                    return Task.FromResult((IEnumerable<IdmResource>)etagRes);
                },
                CreateIdmResource = resource =>
                {
                    createCallCount++;
                    if (createCallCount == 1)
                    {
                        Assert.AreEqual("ObjectTypeDescription", resource.ObjectType);
                        Assert.AreEqual("ETag", resource.GetAttrValue("Name"));
                        return Task.FromResult(new IdmResource { ObjectID = "ETagObjectID" });
                    }
                    if (resource.ObjectType == "AttributeTypeDescription")
                    {
                        return Task.FromResult(new IdmResource { ObjectID = "AttrObjectID" + createCallCount });
                    }
                    if (resource.ObjectType == "BindingDescription")
                    {
                        return Task.FromResult(new IdmResource { ObjectID = "BindingObjectID" + createCallCount });
                    }
                    return Task.FromResult(new IdmResource { ObjectID = "ETagObjID" + createCallCount });
                }

            };

            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");

            // Act
            var result = await it.GetByFilter("/ConstantSpecifier", pageSize: 33, doPagedSearch: true);

            // Assert
            Assert.AreEqual(1, pagedResultsCallCount);
            Assert.AreEqual(1, filterCallCount);
            Assert.AreEqual(16, createCallCount);
            Assert.AreEqual("http://myserver/api/etags/ETagObjID16", result.Headers.GetValues("x-idm-next-link").FirstOrDefault());

            string json = await result.Content.ReadAsStringAsync();
            var content = JsonConvert.DeserializeObject<PagedSearchResults>(json);
            Assert.AreEqual(3, content.Results.Count);
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
