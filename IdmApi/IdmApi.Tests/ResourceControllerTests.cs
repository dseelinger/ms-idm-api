using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using FluentAssertions;
using IdmApi.Controllers;
using IdmNet.Fakes;
using IdmNet.Models;
using Xunit;

namespace IdmApi.Tests
{
    public class ResourceControllerTests
    {
        [Fact]
        public async Task T001_It_can_search_for_resources_without_specifying_a_select_or_sort()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                SearchAsyncSearchCriteriaInt32 = (criteria, num) =>
                {
                    // Assert
                    Assert.DoesNotContain("DisplayName", criteria.Selection);
                    IEnumerable<IdmResource> list = new List<IdmResource> { new IdmResource(), new IdmResource() };
                    return Task.FromResult(list);
                }
            };
            var it = new ResourcesController { Client = stub };

            // Act
            IdmResource[] result = await it.Get("/ObjectTypeDescription");

            // Assert
            Assert.Equal(2, result.Length);
        }

        [Fact]
        public async Task T002_It_can_search_for_resources_and_return_specific_attributes()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                SearchAsyncSearchCriteriaInt32 = (criteria, num) =>
                {
                    // Assert
                    Assert.Contains("DisplayName", criteria.Selection);
                    Assert.Contains("Name", criteria.Selection);
                    IEnumerable<IdmResource> list = new List<IdmResource>
                    {
                        new ObjectTypeDescription { DisplayName = "DisplayName1", Name = "Name1" },
                        new ObjectTypeDescription { DisplayName = "DisplayName2", Name = "Name2" },
                    };
                    return Task.FromResult(list);
                }
            };
            var it = new ResourcesController { Client = stub };

            // Act
            IdmResource[] resources = await (it.Get("/ObjectTypeDescription", "DisplayName,Name"));

            // Assert
            ObjectTypeDescription[] result = resources.Select(res => new ObjectTypeDescription(res)).ToArray();
            Assert.Equal(2, result.Length);
            Assert.Equal("DisplayName1", result[0].DisplayName);
            Assert.Equal("Name1", result[0].Name);
            Assert.Equal("DisplayName2", result[1].DisplayName);
            Assert.Equal("Name2", result[1].Name);
        }

        [Fact]
        public async Task T003_It_can_search_and_return_all_attributes_with_Select_STAR()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                SearchAsyncSearchCriteriaInt32 = (criteria, num) =>
                {
                    // Assert
                    Assert.Contains("*", criteria.Selection);
                    IEnumerable<IdmResource> list = new List<IdmResource>
                    {
                        new ObjectTypeDescription { DisplayName = "DisplayName1", Name = "Name1", Description = "Description1"},
                        new ObjectTypeDescription { DisplayName = "DisplayName2", Name = "Name2", Description = "Description2"},
                    };
                    return Task.FromResult(list);
                }
            };

            // Act
            var it = new ResourcesController { Client = stub };

            await it.Get("/ObjectTypeDescription", "*");
        }

        [Fact]
        public async Task T004_It_can_Search_and_Sort_the_results_by_multiple_attributes_in_Ascending_or_Descending_order()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                SearchAsyncSearchCriteriaInt32 = (criteria, num) =>
                {
                    // Assert
                    Assert.Equal(2, criteria.Sorting.SortingAttributes.Length);
                    Assert.True(criteria.Sorting.SortingAttributes[0].Ascending);
                    Assert.Equal("BoundObjectType", criteria.Sorting.SortingAttributes[0].AttributeName);
                    Assert.False(criteria.Sorting.SortingAttributes[1].Ascending);
                    Assert.Equal("BoundAttributeType", criteria.Sorting.SortingAttributes[1].AttributeName);

                    IEnumerable<IdmResource> list = new List<IdmResource>
                    {
                        new ObjectTypeDescription { DisplayName = "DisplayName1", Name = "Name1", Description = "Description1"},
                        new ObjectTypeDescription { DisplayName = "DisplayName2", Name = "Name2", Description = "Description2"},
                    };
                    return Task.FromResult(list);
                }
            };

            // Act
            var it = new ResourcesController { Client = stub };

            await it.Get("/BindingDescription", "*", "BoundObjectType:Ascending,BoundAttributeType:Descending");
        }

        [Fact]
        public async Task T004point1_It_throws_an_exception_when_the_sort_order_is_not_formatted_properly()
        {
            // Arrange
            var stub = new StubIIdmNetClient();
            var it = new ResourcesController { Client = stub };

            // Act
            var exceptionAsync = Record.ExceptionAsync(() => it.Get("/BindingDescription", "*", "BadSortValue"));
            exceptionAsync.Should().NotBeNull();
            // ReSharper disable once PossibleNullReferenceException
            var ex = await exceptionAsync;

            // Assert
            Assert.NotNull(ex);
            Assert.IsType<HttpResponseException>(ex);
        }

        [Fact]
        public async Task T005_It_can_get_a_resource_by_its_ObjectID()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                GetAsyncStringListOfString = (objectId, selection) =>
                {
                    // Assert
                    Assert.Equal("fb89aefa-5ea1-47f1-8890-abe7797d6497", objectId);
                    Assert.DoesNotContain("DisplayName", selection);
                    return Task.FromResult(new IdmResource { ObjectID = "fb89aefa-5ea1-47f1-8890-abe7797d6497", ObjectType = "Person" });
                }
            };
            var it = new ResourcesController { Client = stub };

            // Act
            IdmResource result = await it.GetById("fb89aefa-5ea1-47f1-8890-abe7797d6497");

            // Assert
            Assert.Equal("Person", result.ObjectType);
        }

        [Fact]
        public async Task T006_It_can_get_any_or_all_attributes_for_a_resource_by_its_ObjectID()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                GetAsyncStringListOfString = (objectId, selection) =>
                {
                    // Assert
                    Assert.Equal("fb89aefa-5ea1-47f1-8890-abe7797d6497", objectId);
                    Assert.Contains("DisplayName", selection);
                    Assert.Contains("Description", selection);
                    return Task.FromResult(new IdmResource { ObjectID = "fb89aefa-5ea1-47f1-8890-abe7797d6497", ObjectType = "Person", DisplayName = "DisplayName1", Description = "Description1"});
                }
            };
            var it = new ResourcesController { Client = stub };

            // Act
            IdmResource result = await it.GetById("fb89aefa-5ea1-47f1-8890-abe7797d6497", "DisplayName,Description");

            // Assert
            Assert.Equal("DisplayName1", result.DisplayName);
        }

        [Fact]
        public async Task T007_It_can_return_the_number_of_matching_records_for_a_given_search()
        {
            // Arrange
            var stub = new StubIIdmNetClient
            {
                GetCountAsyncString = filter =>
                {
                    Assert.Equal("/ConstantSpecifier", filter);
                    return Task.FromResult(97);
                }
            };
            var it = new ResourcesController { Client = stub, Request = new HttpRequestMessage()};

            // Act
            HttpResponseMessage result = await it.Head("/ConstantSpecifier");

            // Assert
            Assert.Equal(97, int.Parse( result.Headers.GetValues("x-idm-count").First()));
        }

        [Fact]
        public async Task T008_It_can_create_objects_in_Identity_Manager()
        {
            // Arrange
            string expectedGuid = new Guid().ToString();
            var stub = new StubIIdmNetClient
            {
                CreateAsyncIdmResource = r =>
                {
                    Assert.Equal("DisplayName1", r.DisplayName);
                    return Task.FromResult(Message.CreateMessage(MessageVersion.Default, "foo"));
                },
                GetNewObjectIdMessage = m =>
                {
                    var s = m.ToString();
                    Assert.True(s != null && s.Contains("foo"));
                    return expectedGuid;
                }
            };
            var it = new ResourcesController { Client = stub, Request = new HttpRequestMessage() };
            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            it.Request.RequestUri = new Uri("http://myserver");
            var resource = new IdmResource {DisplayName = "DisplayName1"};

            // Act
            HttpResponseMessage result = await it.Post(resource);

            // Assert
            Assert.True(result.Headers.Location.AbsoluteUri.EndsWith(expectedGuid));
        }
    }
}


//        [TestMethod]
//        public async Task T008_It_can_create_objects_in_Identity_Manager()
//        {
//            var resource = new IdmResource { DisplayName = "foo" };

//            var repo = new StubIRepository
//            {
//                CreateIdmResource = idmResource =>
//                {
//                    idmResource.ObjectID = "bar";
//                    return Task.FromResult(idmResource);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            HttpResponseMessage result = await it.Post(resource);

//            var json = await result.Content.ReadAsStringAsync();
//            var resourceResult = JsonConvert.DeserializeObject<IdmResource>(json);
//            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
//            Assert.AreEqual("bar", resourceResult.ObjectID);
//            Assert.AreEqual("http://myserver/api/resources/bar", result.Headers.Location.ToString());
//        }

//[TestMethod]
//[TestCategory("Integration")]
//public async Task T008_It_can_create_objects_in_Identity_Manager()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    var newUser = new IdmResource { ObjectType = "Person", DisplayName = "Test User" };

//    // Act
//    Message createResult = await it.CreateAsync(newUser);

//    // assert
//    string objectId = it.GetNewObjectId(createResult);
//    var result = await it.GetAsync(objectId, new List<string> { "DisplayName" });
//    Assert.AreEqual(newUser.DisplayName, result.DisplayName);

//    // afterwards
//    await it.DeleteAsync(objectId);
//}







//[TestMethod]
//[TestCategory("Integration")]
//public async Task T009_It_can_delete_objects_from_Identity_Manager()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    Message toDelete =
//        await it.CreateAsync(new IdmResource { ObjectType = "Person", DisplayName = "Test User" });
//    var objectId = it.GetNewObjectId(toDelete);

//    // Act
//    Message result = await it.DeleteAsync(objectId);


//    // Assert
//    Assert.IsFalse(result.IsFault);
//    try
//    {
//        await it.GetAsync(objectId, new List<string> { "DisplayName" });
//        Assert.Fail("Should not make it here");
//    }
//    catch (KeyNotFoundException)
//    {
//        // OK
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task
//    T010_It_can_do_a_search_and_return_the_first_page_of_results_and_info_on_retrieving_subsequent_pages_if_any()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    var criteria = new SearchCriteria("/ObjectTypeDescription");
//    criteria.Selection.Add("DisplayName");

//    // Act
//    PagedSearchResults result = await it.GetPagedResultsAsync(criteria, 5);

//    // Assert
//    Assert.AreEqual("/ObjectTypeDescription", result.PagingContext.Filter);
//    Assert.AreEqual(5, result.PagingContext.CurrentIndex);
//    Assert.AreEqual("Forwards", result.PagingContext.EnumerationDirection);
//    Assert.AreEqual("9999-12-31T23:59:59.9999999", result.PagingContext.Expires);
//    Assert.AreEqual("ObjectID", result.PagingContext.Selection[0]);
//    Assert.AreEqual("ObjectType", result.PagingContext.Selection[1]);
//    Assert.AreEqual("DisplayName", result.PagingContext.Selection[2]);
//    Assert.AreEqual("DisplayName", result.PagingContext.Sorting.SortingAttributes[0].AttributeName);
//    Assert.AreEqual(true, result.PagingContext.Sorting.SortingAttributes[0].Ascending);

//    Assert.AreEqual("ObjectTypeDescription", result.Results[0].ObjectType);
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task T011_It_can_get_resources_back_from_a_search_a_page_at_a_time()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    var criteria = new SearchCriteria("/ObjectTypeDescription");
//    criteria.Selection.Add("DisplayName");
//    PagedSearchResults pagedSearchResults = await it.GetPagedResultsAsync(criteria, 5);
//    PagingContext pagingContext = pagedSearchResults.PagingContext;

//    // Act
//    var pagedResults = await it.GetPagedResultsAsync(5, pagingContext);

//    // Assert
//    Assert.AreEqual(5, pagedResults.Results.Count);
//    Assert.IsTrue(pagedResults.Results[0].DisplayName.Length > 0);
//    Assert.AreEqual("/ObjectTypeDescription", pagedResults.PagingContext.Filter);
//    Assert.AreEqual(10, pagedResults.PagingContext.CurrentIndex);
//    Assert.AreEqual("Forwards", pagedResults.PagingContext.EnumerationDirection);
//    Assert.AreEqual("9999-12-31T23:59:59.9999999", pagedResults.PagingContext.Expires);
//    Assert.AreEqual("ObjectID", pagedResults.PagingContext.Selection[0]);
//    Assert.AreEqual("ObjectType", pagedResults.PagingContext.Selection[1]);
//    Assert.AreEqual("DisplayName", pagedResults.PagingContext.Selection[2]);
//    Assert.AreEqual("DisplayName", pagedResults.PagingContext.Sorting.SortingAttributes[0].AttributeName);
//    Assert.AreEqual(true, pagedResults.PagingContext.Sorting.SortingAttributes[0].Ascending);

//    Assert.AreEqual(null, pagedResults.EndOfSequence);
//    Assert.AreEqual(true, pagedResults.Items is XmlNode[]);
//    Assert.AreEqual(5, ((XmlNode[])(pagedResults.Items)).Length);
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task T012_It_can_approve_requests()
//{
//    // Note: for this test to pass you have to enable the MPRs for Distribution Group Management, and run from a machine with MIM on it.
//    // Arrange
//    TestUserInfo ownerUser = null;
//    TestUserInfo joiningUser = null;
//    string groupId = "";
//    try
//    {
//        ownerUser = await CreateTestUser("Owner01");
//        joiningUser = await CreateTestUser("Joiner01");

//        try
//        {
//            groupId = await CreateGroup(ownerUser, "02");

//            var approvals = await GenerateApproval(joiningUser, groupId);
//            string fqdn = IdmNetClientFactory.GetEnvironmentSetting("MIM_fqdn");
//            var ownerClient =
//                IdmNetClientFactory.BuildClient();


//            // Act
//            var result = await ownerClient.ApproveAsync(approvals[0]);

//            // Assert
//            //AssertUserIsInGroupNow();
//        }
//        finally
//        {
//            DeleteGroup(groupId);
//        }
//    }
//    finally
//    {
//        // Afterwards
//        await DeleteUser(joiningUser);
//        await DeleteUser(ownerUser);
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task T013_It_can_return_the_approval_objects_associated_with_a_particular_request()
//{
//    // Note: for this test to pass you have to enable the MPRs for Distribution Group Management, and run from a machine with MIM on it.
//    // Arrange
//    TestUserInfo ownerUser = null;
//    TestUserInfo joiningUser = null;
//    string groupId = "";
//    try
//    {
//        ownerUser = await CreateTestUser("Owner01");
//        joiningUser = await CreateTestUser("Joiner01");

//        try
//        {
//            groupId = await CreateGroup(ownerUser, "02");

//            // Act
//            var approvals = await GenerateApproval(joiningUser, groupId);

//            // Assert
//            Assert.IsNotNull(approvals);
//            Assert.AreEqual(1, approvals.Count);
//            Assert.IsNotNull(approvals[0].EndpointAddress);
//            Assert.IsNotNull(approvals[0].WorkflowInstance);

//        }
//        finally
//        {
//            DeleteGroup(groupId);
//        }
//    }
//    finally
//    {
//        // Afterwards
//        await DeleteUser(joiningUser);
//        await DeleteUser(ownerUser);
//    }
//}

//[TestMethod]
//public async Task It_throws_when_ApproveOrReject_is_called_with_a_null_approval()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    try
//    {
//        await it.ApproveOrRejectAsync(null, "because I said so", true);
//        Assert.Fail("Should not reach here");
//    }
//    catch (ArgumentNullException)
//    {
//        // OK
//    }
//    // Assert
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_return_the_entire_schema_for_a_particular_object_type()
//{
//    // Arrange
//    var personOid = "6cb7e506-b4b3-4901-8b8c-ff044f14e743";
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    Schema result = await it.GetSchemaAsync("Person");

//    //// Assert
//    Assert.AreEqual("User", result.DisplayName);
//    Assert.IsTrue(result.CreatedTime >= new DateTime(2010, 1, 1));
//    Assert.AreEqual(null, result.Creator);
//    Assert.AreEqual("This resource defines applicable policies to manage incoming requests. ",
//        result.Description);
//    Assert.AreEqual("Person", result.Name);
//    Assert.AreEqual(personOid, result.ObjectID);
//    Assert.AreEqual("ObjectTypeDescription", result.ObjectType);
//    Assert.AreEqual(null, result.ResourceTime);
//    Assert.AreEqual("Microsoft.ResouceManagement.PortalClient", result.UsageKeyword[0]);

//    var expectedBindingCount = 59;
//    Assert.IsTrue(result.BindingDescriptions.Count >= expectedBindingCount);
//    for (int i = 0; i < expectedBindingCount; i++)
//    {
//        Assert.AreEqual(personOid, result.BindingDescriptions[i].BoundObjectType.ObjectID);
//    }
//    Assert.AreEqual("3e04bbbf-014f-413c-8d07-6276cd383be8",
//        result.BindingDescriptions[0].BoundAttributeType.ObjectID);
//    Assert.AreEqual(false, result.BindingDescriptions[0].Required);

//    Assert.AreEqual("String", result.BindingDescriptions[0].BoundAttributeType.DataType);
//    Assert.AreEqual(false, result.BindingDescriptions[0].BoundAttributeType.Multivalued);
//    Assert.AreEqual("Account Name", result.BindingDescriptions[0].BoundAttributeType.DisplayName);
//    Assert.AreEqual("User's log on name", result.BindingDescriptions[0].BoundAttributeType.Description);
//    Assert.AreEqual(null, result.BindingDescriptions[0].BoundAttributeType.IntegerMaximum);
//    Assert.AreEqual(null, result.BindingDescriptions[0].BoundAttributeType.IntegerMinimum);
//    Assert.AreEqual("AccountName", result.BindingDescriptions[0].BoundAttributeType.Name);
//    Assert.AreEqual(null, result.BindingDescriptions[0].BoundAttributeType.StringRegex);
//    Assert.AreEqual("Microsoft.ResourceManagement.WebServices",
//        result.BindingDescriptions[0].BoundAttributeType.UsageKeyword[0]);
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_get_resources_even_without_an_initial_search_call_if_you_know_what_you_are_doing()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    PagingContext pagingContext = new PagingContext
//    {
//        CurrentIndex = 0,
//        Filter = "/ObjectTypeDescription",
//        Selection = new[] { "ObjectID", "ObjectType", "DisplayName" },
//        Sorting = new Sorting(),
//        EnumerationDirection = "Forwards",
//        Expires = "9999-12-31T23:59:59.9999999"
//    };

//    // Act
//    var pagedResults = await it.GetPagedResultsAsync(5, pagingContext);

//    // Assert
//    Assert.AreEqual(5, pagedResults.Results.Count);
//    Assert.IsTrue(pagedResults.Results[0].DisplayName.Length > 0);

//    Assert.AreEqual(null, pagedResults.EndOfSequence);
//    Assert.AreEqual(true, pagedResults.Items is XmlNode[]);
//    Assert.AreEqual(5, ((XmlNode[])(pagedResults.Items)).Length);
//}

//[TestMethod]
//[TestCategory("Integration")]
//[ExpectedException(typeof(SoapFaultException))]
//public async Task It_throws_an_exception_when_it_encounters_bad_xpath()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    var criteria = new SearchCriteria("<3#");

//    // Act
//    await it.SearchAsync(criteria);
//}

//[TestMethod]
//[TestCategory("Integration")]
//[ExpectedException(typeof(SoapFaultException))]
//public async Task It_throws_an_exception_when_valid_yet_unknown_xpath_is_searched_for()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    var criteria = new SearchCriteria("/foo");

//    // Act
//    await it.SearchAsync(criteria);
//}

//[TestMethod]
//[ExpectedException(typeof(ArgumentNullException))]
//public async Task It_throws_when_passing_a_null_resource_to_create()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    await it.CreateAsync(null);
//}

//[TestMethod]
//[TestCategory("Integration")]
//[ExpectedException(typeof(SoapFaultException))]
//public async Task It_throws_an_exception_when_trying_to_create_an_invalid_resource()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    var newUser = new IdmResource();
//    await it.CreateAsync(newUser);
//}

//[TestMethod]
//[ExpectedException(typeof(ArgumentNullException))]
//public async Task It_throws_when_attempting_to_delete_a_null_ObjectID()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    await it.DeleteAsync(null);
//}

//[TestMethod]
//[TestCategory("Integration")]
//[ExpectedException(typeof(SoapFaultException))]
//public async Task It_throws_an_exception_when_attempting_to_delete_an_object_that_does_not_exist()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    await it.DeleteAsync("bad object id");
//}

//[TestMethod]
//[TestCategory("Integration")]
//[ExpectedException(typeof(SoapFaultException))]
//public async Task It_throws_an_exception_when_you_treat_a_single_valued_attribute_as_if_it_is_multi_valued()
//{
//    // Arrange
//    const string attrName = "FirstName";
//    const string attrValue = "Testing";
//    var it = IdmNetClientFactory.BuildClient();
//    Message testResource = await CreateTestPerson(it);
//    var objectId = it.GetNewObjectId(testResource);

//    // Act
//    try
//    {
//        Message result = await it.AddValueAsync(objectId, attrName, attrValue);

//        Assert.IsFalse(result.IsFault);
//    }
//    finally
//    {
//        // Afterwards
//        it.DeleteAsync(objectId);
//    }

//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_add_a_value_to_a_multi_valued_attribute_that_already_has_one_or_more_values
//    ()
//{
//    // Arrange
//    const string attrName = "SearchScopeContext";
//    const string attrValue = "FirstName";
//    var it = IdmNetClientFactory.BuildClient();
//    Message testResource = await CreateTestSearchScope(it);
//    var objectId = it.GetNewObjectId(testResource);

//    // Act
//    try
//    {
//        await it.AddValueAsync(objectId, attrName, attrValue);

//        // Assert
//        var searchResult =
//            await
//                it.SearchAsync(new SearchCriteria
//                {
//                    Filter =
//                        new Filter
//                        {
//                            Query = "/SearchScopeConfiguration[ObjectID='" + objectId + "']"
//                        },
//                    Selection = new List<string> { "SearchScopeContext" }
//                });
//        Assert.IsTrue(searchResult.First().GetAttrValues(attrName).Contains(attrValue));
//    }
//    finally
//    {
//        // Afterwards
//        it.DeleteAsync(objectId);
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_add_a_value_to_an_empty_multi_valued_attribute()
//{
//    // Arrange
//    const string attrName = "ProxyAddressCollection";
//    const string attrValue = "joecool@nowhere.net";
//    var it = IdmNetClientFactory.BuildClient();
//    Message testResource = await CreateTestPerson(it);
//    var objectId = it.GetNewObjectId(testResource);

//    // Act
//    try
//    {
//        await it.AddValueAsync(objectId, attrName, attrValue);

//        // Assert
//        var searchResult =
//            await
//                it.SearchAsync(new SearchCriteria
//                {
//                    Filter =
//                        new Filter
//                        {
//                            Query = "/Person[ObjectID='" + objectId + "']"
//                        },
//                    Selection = new List<string> { "ProxyAddressCollection" }
//                });

//        Assert.IsTrue(searchResult.First().GetAttrValues(attrName).Contains(attrValue));
//    }
//    finally
//    {
//        // Afterwards
//        it.DeleteAsync(objectId);
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_delete_a_value_from_a_multi_valued_attribute()
//{
//    // Arrange
//    const string attrName = "ProxyAddressCollection";
//    const string attrValue1 = "joecool@nowhere.net";
//    const string attrValue2 = "joecool@nowhere.lab";
//    var it = IdmNetClientFactory.BuildClient();
//    Message testResource = await CreateTestPerson(it);
//    var objectId = it.GetNewObjectId(testResource);

//    try
//    {
//        await it.AddValueAsync(objectId, attrName, attrValue1);
//        await it.AddValueAsync(objectId, attrName, attrValue2);

//        // Act
//        Message result = await it.RemoveValueAsync(objectId, attrName, attrValue2);

//        // Assert
//        Assert.IsFalse(result.IsFault);
//        var searchResult =
//            await
//                it.SearchAsync(new SearchCriteria
//                {
//                    Filter =
//                        new Filter
//                        {
//                            Query = "/Person[ObjectID='" + objectId + "']"
//                        },
//                    Selection = new List<string> { "ProxyAddressCollection" }
//                });

//        Assert.IsFalse(searchResult.First().GetAttrValues(attrName).Contains(attrValue2));
//    }
//    finally
//    {
//        // Afterwards
//        it.DeleteAsync(objectId);
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_modify_single_valued_attribute_that_was_previously_null()
//{
//    // Arrange
//    const string attrName = "FirstName";
//    const string attrValue = "TestFirstName";
//    var it = IdmNetClientFactory.BuildClient();
//    Message testResource = await CreateTestPerson(it);
//    var objectId = it.GetNewObjectId(testResource);

//    try
//    {
//        // Act
//        Message result = await it.ReplaceValueAsync(objectId, attrName, attrValue);

//        // Assert
//        Assert.IsFalse(result.IsFault);
//        var searchResult =
//            await
//                it.SearchAsync(new SearchCriteria
//                {
//                    Filter =
//                        new Filter
//                        {
//                            Query = "/Person[ObjectID='" + objectId + "']"
//                        },
//                    Selection = new List<string> { attrName }
//                });

//        Assert.AreEqual(attrValue, searchResult.First().GetAttrValue(attrName));
//    }
//    finally
//    {
//        // Afterwards
//        it.DeleteAsync(objectId);
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_modify_a_value_of_a_single_valued_attribute_even_if_it_already_had_a_value()
//{
//    await AssertReplaceOk("FirstName", "TestFirstName1", "TestFirstName2");
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_remove_a_single_valued_attribute_by_setting_its_value_to_null()
//{
//    await AssertReplaceOk("FirstName", "TestFirstName1", null);
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_can_make_a_bunch_of_changes_at_the_same_time_for_a_single_resource()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    Message testResource = await CreateTestPerson(it);
//    var objectId = it.GetNewObjectId(testResource);
//    var changes1 = new[]
//    {
//                new Change(ModeType.Replace, "FirstName", "FirstNameTest"),
//                new Change(ModeType.Replace, "LastName", "LastNameTest"),
//                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab1.lab"),
//                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab2.lab"),
//            };

//    try
//    {
//        // Act
//        Message result = await it.ChangeMultipleAttrbutes(objectId, changes1);

//        // Assert
//        Assert.IsFalse(result.IsFault);
//        var searchResult =
//            await
//                it.SearchAsync(new SearchCriteria
//                {
//                    Filter =
//                        new Filter
//                        {
//                            Query = "/Person[ObjectID='" + objectId + "']"
//                        },
//                    Selection = new List<string> { "FirstName", "LastName", "ProxyAddressCollection" }
//                });

//        var modifiedResource1 = searchResult.First();
//        Assert.AreEqual("FirstNameTest", modifiedResource1.GetAttrValue("FirstName"));
//        Assert.AreEqual("LastNameTest", modifiedResource1.GetAttrValue("LastName"));
//        Assert.IsTrue(modifiedResource1.GetAttrValues("ProxyAddressCollection").Contains("joe@lab1.lab"));
//        Assert.IsTrue(modifiedResource1.GetAttrValues("ProxyAddressCollection").Contains("joe@lab2.lab"));
//    }
//    finally
//    {
//        // Afterwards
//        it.DeleteAsync(objectId);
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_returns_the_same_number_for_both_GetCount_and_Search()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    int count = await it.GetCountAsync("/ConstantSpecifier");

//    var searchResults = await it.SearchAsync(new SearchCriteria("/ConstantSpecifier"), 25);

//    Assert.AreEqual(count, searchResults.Count());
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_doesnt_add_superflous_attributes_on_create()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();
//    string newObjectId = null;

//    try
//    {
//        var newUser = new IdmResource
//        {
//            Attributes = new List<IdmAttribute>
//                    {
//                        new IdmAttribute {Name = "ObjectType", Values = new List<string> {"Person"}},
//                        new IdmAttribute {Name = "ObjectID", Values = new List<string>()},
//                        new IdmAttribute {Name = "DisplayName", Values = new List<string> {"_Test User"}},
//                    }
//        };
//        var createResult = await it.CreateAsync(newUser);

//        // assert
//        newObjectId = it.GetNewObjectId(createResult);
//        var result = await it.GetAsync(newObjectId, new List<string> { "DisplayName" });
//        Assert.AreEqual(newUser.DisplayName, result.DisplayName);
//    }
//    finally
//    {
//        // afterwards
//        it.DeleteAsync(newObjectId);
//    }
//    // Act
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_returns_0_records_when_no_records_match_search()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    var searchResults = await it.SearchAsync(new SearchCriteria("/Configuration"), 25);

//    Assert.AreEqual(0, searchResults.Count());
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_returns_0_records_when_no_records_match_SELECT_STAR_search()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    var searchResults =
//        await it.SearchAsync(new SearchCriteria("/Configuration") { Selection = new List<string> { "*" } }, 25);

//    Assert.AreEqual(0, searchResults.Count());
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_throws_an_exception_when_GetAsync_is_called_but_no_match_for_the_object_ID()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    try
//    {
//        IdmResource result = await it.GetAsync("c51c9ef3-f00d-4d4e-b30b-c1a18e79c56e", null);
//        Assert.IsTrue(false, "Should have encountered a KeyNotFoundException");
//    }
//    catch (KeyNotFoundException)
//    {
//        // Expected Exception
//    }
//}

//[TestMethod]
//[TestCategory("Integration")]
//public async Task It_throws_an_exception_when_GetAsync_is_called_but_no_match_for_the_object_ID_and_SELECT_STAR()
//{
//    // Arrange
//    var it = IdmNetClientFactory.BuildClient();

//    // Act
//    try
//    {
//        IdmResource result = await it.GetAsync("c51c9ef3-f00d-4d4e-b30b-c1a18e79c56e", new List<string> { "*" });
//        Assert.IsTrue(false, "Should have encountered a KeyNotFoundException");
//    }
//    catch (KeyNotFoundException)
//    {
//        // Expected Exception
//    }
//}

//[TestMethod]
//public void It_can_return_a_ReferenceResourceProperty_if_one_is_present_in_a_SOAP_Message_string()

// TODO 007: Implement /api/persons
// TODO 006: Implement /api/groups
// TODO 005: Implement /api/attributetypedescriptions
// TODO 004: Implement /api/objecttypedescriptions
// TODO 003: Implement /api/bindingdescriptions
// TODO 002: Implement /api/whatever
// TODO 001: Implement Approvals
// TODO -999: Implement the STS endpoint

//        [TestMethod]
//        public async Task T009_It_can_delete_objects_from_Identity_Manager()
//        {
//            // Arrange
//            var repo = new StubIRepository
//            {
//                DeleteResourceString = objId =>
//                {
//                    Assert.AreEqual("id", objId);

//                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
//                    return Task.FromResult(msg);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            // Act
//            HttpResponseMessage result = await it.DeleteResource("id");

//            // Assert
//            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
//        }

//        [TestMethod]
//        public async Task T010_It_can_do_a_search_and_return_the_first_page_of_results_and_info_on_retrieving_subsequent_pages_if_any()
//        {
//            // Arrange
//            const string filter = "/ConstantSpecifier";
//            PagedSearchResults pagedResults = new PagedSearchResults
//            {
//                EndOfSequence = null,
//                PagingContext =
//                    new PagingContext
//                    {
//                        CurrentIndex = 25,
//                        EnumerationDirection = "Forwards",
//                        Expires = "some time in the distant future",
//                        Filter = "/ConstantSpecifier",
//                        Selection = new[] { "DisplayName" },
//                        Sorting = new Sorting()
//                    },
//                Items = new object(),
//                Results = new List<IdmResource>
//                {
//                    new IdmResource(),
//                    new IdmResource(),
//                    new IdmResource()
//                }
//            };
//            var etagRes = new List<IdmResource>
//            {
//                new IdmResource()
//            };

//            var pagedResultsCallCount = 0;
//            var filterCallCount = 0;
//            var createCallCount = 0;
//            var repo = new StubIRepository
//            {
//                GetPagedResultsSearchCriteriaInt32 = (criteria, pageSize) =>
//                {
//                    pagedResultsCallCount++;
//                    Assert.AreEqual(33, pageSize);
//                    return Task.FromResult(pagedResults);
//                },
//                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
//                {
//                    filterCallCount++;
//                    Assert.AreEqual(1, pageSize);
//                    Assert.AreEqual("/ObjectTypeDescription[Name='ETag']", criteria.Filter.Query);
//                    return Task.FromResult((IEnumerable<IdmResource>)etagRes);
//                },
//                CreateIdmResource = resource =>
//                {
//                    createCallCount++;
//                    Assert.AreEqual("ETag", resource.ObjectType);
//                    return Task.FromResult(new IdmResource { ObjectID = "foo" });
//                }

//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            // Act
//            var result = await it.GetByFilter(filter, pageSize: 33, doPagedSearch: true);

//            // Assert
//            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
//            Assert.AreEqual(1, pagedResultsCallCount);
//            Assert.AreEqual(1, filterCallCount);
//            Assert.AreEqual(1, createCallCount);
//            Assert.AreEqual("http://myserver/api/etags/foo", result.Headers.GetValues("x-idm-next-link").FirstOrDefault());

//            string json = await result.Content.ReadAsStringAsync();
//            var content = JsonConvert.DeserializeObject<PagedSearchResults>(json);
//            Assert.AreEqual(3, content.Results.Count);
//        }

//        // Test what happens when the ETag ObjectType doesn't exist
//        // ETags endpoint
//        // T011_It_can_get_resources_back_from_a_search_a_page_at_a_time
//        // Should return 404 for "not found" stuff

//        [TestMethod]
//        public async Task It_can_add_a_value_to_a_multi_valued_attribute_that_already_has_one_or_more_values
//            ()
//        {
//            var objectId = Guid.NewGuid().ToString("D");
//            var attrName = "ProxyAddressesCollection";
//            var newValue = "joecool@snoopy.com";

//            // Arrange
//            var repo = new StubIRepository
//            {
//                PostAttributeStringStringString = (objId, name, val) =>
//                {
//                    Assert.AreEqual(objectId, objId);
//                    Assert.AreEqual(attrName, name);
//                    Assert.AreEqual(newValue, val);

//                    var msg = Message.CreateMessage(MessageVersion.Default, "Doesn't matter");
//                    return Task.FromResult(msg);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            // Act
//            HttpResponseMessage result = await it.PostAttribute(objectId, attrName, newValue);

//            // Assert
//            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
//            Assert.AreEqual(string.Format("http://myserver/api/resources/{0}/{1}", objectId, attrName), result.Headers.Location.ToString());
//        }

//        [TestMethod]
//        public void It_has_a_CTOR_that_takes_a_repo()
//        {
//            var repo = new StubIRepository();

//            var it = new ResourcesController(repo);

//            Assert.AreEqual(repo, it.Repo);
//        }

//        [TestMethod]
//        public async Task It_can_GetById_with_no_select()
//        {
//            var idmResource = new IdmResource { DisplayName = "foo" };
//            var repo = new StubIRepository
//            {
//                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
//            };

//            var it = new ResourcesController(repo);

//            var result = await it.GetById("foo");

//            Assert.AreEqual(idmResource, result);
//        }

//        [TestMethod]
//        public async Task It_can_GetById_with_select()
//        {
//            var repo = new StubIRepository
//            {
//                GetByIdStringListOfString = (s, strings) =>
//                {
//                    Assert.AreEqual("bar", strings[0]);
//                    Assert.AreEqual("bat", strings[1]);
//                    return Task.FromResult(new IdmResource());
//                }
//            };

//            var it = new ResourcesController(repo);

//            await it.GetById("foo", "bar,bat");
//        }

//        [TestMethod]
//        public async Task It_can_GetById_with_sloppy_select()
//        {
//            var repo = new StubIRepository
//            {
//                GetByIdStringListOfString = (s, strings) =>
//                {
//                    Assert.AreEqual("bar", strings[0]);
//                    Assert.AreEqual("bat", strings[1]);
//                    return Task.FromResult(new IdmResource());
//                }
//            };

//            var it = new ResourcesController(repo);

//            await it.GetById("foo", " bar, bat ");
//        }

//        [TestMethod]
//        public async Task It_can_GetAttributeById()
//        {
//            var idmResource = new IdmResource
//            {
//                Attributes = new List<IdmAttribute> { new IdmAttribute { Name = "bar", Value = "bat" } }
//            };
//            var repo = new StubIRepository
//            {
//                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
//            };

//            var it = new ResourcesController(repo);

//            var result = (JObject)await it.GetAttributeById("foo", "bar");

//            Assert.AreEqual("bat", result["bar"]);
//        }

//        [TestMethod]
//        public async Task It_can_GetAttributeById_for_multi_valued_atttributes()
//        {
//            var idmResource = new IdmResource
//            {
//                Attributes =
//                    new List<IdmAttribute> { new IdmAttribute { Name = "bar", Values = new List<string> { "fiz", "buz" } } }
//            };
//            var repo = new StubIRepository
//            {
//                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
//            };

//            var it = new ResourcesController(repo);

//            var result = (JObject)await it.GetAttributeById("foo", "bar");

//            Assert.AreEqual("fiz", result["bar"][0]);
//            Assert.AreEqual("buz", result["bar"][1]);
//        }

//        [TestMethod]
//        public async Task It_can_GetAttributeById_for_null_attributes()
//        {
//            var idmResource = new IdmResource
//            {
//                Attributes =
//                    new List<IdmAttribute>()
//            };
//            var repo = new StubIRepository
//            {
//                GetByIdStringListOfString = (s, strings) => Task.FromResult(idmResource)
//            };

//            var it = new ResourcesController(repo);

//            var result = (JObject)await it.GetAttributeById("foo", "bar");

//            Assert.IsNull(result);
//        }


//        [TestMethod]
//        public async Task It_can_PutAttribute()
//        {
//            var repo = new StubIRepository
//            {
//                PutAttributeStringStringString = (objId, name, val) =>
//                {
//                    Assert.AreEqual("foo", objId);
//                    Assert.AreEqual("bar", name);
//                    Assert.AreEqual("bat", val);

//                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
//                    return Task.FromResult(msg);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");


//            HttpResponseMessage result = await it.PutAttribute("foo", "bar", "bat");

//            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
//        }

//        [TestMethod]
//        public async Task It_can_PostAttribute()
//        {
//            // Arrange
//            var repo = new StubIRepository
//            {
//                PostAttributeStringStringString = (objId, name, val) =>
//                {
//                    Assert.AreEqual("foo", objId);
//                    Assert.AreEqual("bar", name);
//                    Assert.AreEqual("bat", val);

//                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
//                    return Task.FromResult(msg);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            // Act
//            HttpResponseMessage result = await it.PostAttribute("foo", "bar", "bat");

//            // Assert
//            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
//            Assert.AreEqual("http://myserver/api/resources/foo/bar", result.Headers.Location.ToString());
//        }

//        [TestMethod]
//        public async Task It_can_DeleteAttribute()
//        {
//            // Arrange
//            var repo = new StubIRepository
//            {
//                DeleteAttributeStringStringString = (objId, name, val) =>
//                {
//                    Assert.AreEqual("foo", objId);
//                    Assert.AreEqual("bar", name);
//                    Assert.AreEqual("bat", val);

//                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
//                    return Task.FromResult(msg);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            // Act
//            HttpResponseMessage result = await it.DeleteAttribute("foo", "bar", "bat");

//            // Assert
//            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
//        }


//        [TestMethod]
//        public async Task It_can_PutChanges()
//        {
//            var changes1 = new[]
//            {
//                new Change(ModeType.Replace, "FirstName", "FirstNameTest"),
//                new Change(ModeType.Replace, "LastName", "LastNameTest"),
//                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab1.lab"),
//                new Change(ModeType.Add, "ProxyAddressCollection", "joe@lab2.lab"),
//            };

//            var repo = new StubIRepository
//            {
//                PutChangesStringChangeArray = (objId, changes) =>
//                {
//                    Assert.AreEqual("foo", objId);
//                    Assert.AreEqual(changes1, changes);

//                    var msg = Message.CreateMessage(MessageVersion.Default, "foo");
//                    return Task.FromResult(msg);
//                }
//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            HttpResponseMessage result = await it.PutChanges("foo", changes1);

//            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
//        }


//        [TestMethod]
//        public async Task It_can_do_a_search_and_return_the_first_page_of_results_and_info_on_retrieving_subsequent_pages_even_if_ETag_doesnt_exist_in_FIM()
//        {
//            // Arrange
//            PagedSearchResults pagedResults = new PagedSearchResults
//            {
//                EndOfSequence = null,
//                PagingContext =
//                    new PagingContext
//                    {
//                        CurrentIndex = 25,
//                        EnumerationDirection = "Forwards",
//                        Expires = "some time in the distant future",
//                        Filter = "/ConstantSpecifier",
//                        Selection = new[] { "DisplayName" },
//                        Sorting = new Sorting()
//                    },
//                Items = new object(),
//                Results = new List<IdmResource>
//                {
//                    new IdmResource(),
//                    new IdmResource(),
//                    new IdmResource()
//                }
//            };
//            var etagRes = new List<IdmResource> { };

//            var pagedResultsCallCount = 0;
//            var filterCallCount = 0;
//            var createCallCount = 0;
//            var repo = new StubIRepository
//            {
//                GetPagedResultsSearchCriteriaInt32 = (criteria, pageSize) =>
//                {
//                    pagedResultsCallCount++;
//                    Assert.AreEqual(33, pageSize);
//                    return Task.FromResult(pagedResults);
//                },
//                GetByFilterSearchCriteriaInt32 = (criteria, pageSize) =>
//                {
//                    filterCallCount++;
//                    Assert.AreEqual(1, pageSize);
//                    Assert.AreEqual("/ObjectTypeDescription[Name='ETag']", criteria.Filter.Query);
//                    return Task.FromResult((IEnumerable<IdmResource>)etagRes);
//                },
//                CreateIdmResource = resource =>
//                {
//                    createCallCount++;
//                    if (createCallCount == 1)
//                    {
//                        Assert.AreEqual("ObjectTypeDescription", resource.ObjectType);
//                        Assert.AreEqual("ETag", resource.GetAttrValue("Name"));
//                        return Task.FromResult(new IdmResource { ObjectID = "ETagObjectID" });
//                    }
//                    if (resource.ObjectType == "AttributeTypeDescription")
//                    {
//                        return Task.FromResult(new IdmResource { ObjectID = "AttrObjectID" + createCallCount });
//                    }
//                    if (resource.ObjectType == "BindingDescription")
//                    {
//                        return Task.FromResult(new IdmResource { ObjectID = "BindingObjectID" + createCallCount });
//                    }
//                    return Task.FromResult(new IdmResource { ObjectID = "ETagObjID" + createCallCount });
//                }

//            };

//            var it = new ResourcesController(repo) { Request = new HttpRequestMessage() };
//            it.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
//            it.Request.RequestUri = new Uri("http://myserver");

//            // Act
//            var result = await it.GetByFilter("/ConstantSpecifier", pageSize: 33, doPagedSearch: true);

//            // Assert
//            Assert.AreEqual(1, pagedResultsCallCount);
//            Assert.AreEqual(1, filterCallCount);
//            Assert.AreEqual(16, createCallCount);
//            Assert.AreEqual("http://myserver/api/etags/ETagObjID16", result.Headers.GetValues("x-idm-next-link").FirstOrDefault());

//            string json = await result.Content.ReadAsStringAsync();
//            var content = JsonConvert.DeserializeObject<PagedSearchResults>(json);
//            Assert.AreEqual(3, content.Results.Count);
//        }

//    }
//}


