using System;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using IdmNet.Models;
using RestSharp;
using Xunit;

namespace IdmApi.Tests
{
    public class IisExpressFixture : IDisposable
    {
        public Process IisExpress { get; }

        public IisExpressFixture()
        {
            IisExpress = Process.Start("c:\\Program Files\\IIS Express\\iisexpress.exe", "/path:C:\\git\\ms-idm-api\\IdmApi\\IdmApi /port:8088");
        }

        public void Dispose()
        {
            IisExpress.Kill();
        }

    }

    public class IntegrationTests : IClassFixture<IisExpressFixture>
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly IisExpressFixture _fixture;
        private readonly RestClient _client;
        private const string ResourceEndpoint = "api/resources";
        private const string BaseUrl = "http://localhost:8088/";
        

        public IntegrationTests(IisExpressFixture fixture)
        {
            _fixture = fixture;
            _client = new RestClient(BaseUrl);
        }

        [Fact]
        public void T001_It_can_search_for_resources_without_specifying_a_select_or_sort()
        {
            // Arrange
            var request = BuildGetRequest();
            request.AddParameter("filter", "/ObjectTypeDescription");

            // Act
            var restResponse = _client.Execute<List<IdmResource>>(request);

            // Assert
            restResponse.Data.Count.Should().BeGreaterOrEqualTo(40);
        }

        [Fact]
        public void T002_It_can_search_for_resources_and_return_specific_attributes()
        {
            // Arrange
            var request = BuildGetRequest();
            request.AddParameter("filter", "/ObjectTypeDescription");
            request.AddParameter("select", "DisplayName,Name");

            // Act
            var restResponse = _client.Execute<List<IdmResource>>(request);

            // Assert
            var firstResource = new ObjectTypeDescription(restResponse.Data[0]);
            firstResource.DisplayName.Should().Be("Activity Information Configuration");
            firstResource.Name.Should().Be("ActivityInformationConfiguration");

            var secondResource = new ObjectTypeDescription(restResponse.Data[1]);
            secondResource.DisplayName.Should().Be("Approval");
            secondResource.Name.Should().Be("Approval");
        }

        [Fact]
        public void T003_It_can_search_and_return_all_attributes_with_Select_STAR()
        {
            // Arrange
            var request = BuildGetRequest();
            request.AddParameter("filter", "/ObjectTypeDescription");
            request.AddParameter("select", "*");

            // Act
            var restResponse = _client.Execute<List<IdmResource>>(request);

            // Assert
            var firstResource = new ObjectTypeDescription(restResponse.Data[0]);
            firstResource.Attributes.Count.Should().BeGreaterOrEqualTo(7);
        }

        [Fact]
        public void T004_It_can_Search_and_Sort_the_results_by_multiple_attributes_in_Ascending_or_Descending_order()
        {
            // Arrange
            var request = BuildGetRequest();
            request.AddParameter("filter", "/BindingDescription");
            request.AddParameter("select", "*");
            request.AddParameter("sort", "BoundObjectType:Ascending,BoundAttributeType:Descending");

            // Act
            var restResponse = _client.Execute<List<IdmResource>>(request);

            // Assert
            AssertBindingAsExpected(
                restResponse.Data[0],
                expectedObjectType: "ActivityInformationConfiguration",
                expectedAttrType: "TypeName");

            AssertBindingAsExpected(restResponse.Data[restResponse.Data.Count - 1], "msidmPamConfiguration", "ObjectID");
        }

        [Fact]
        public void T004point1_It_throws_an_exception_when_the_sort_order_is_not_formatted_properly()
        {
            // Arrange
            var request = BuildGetRequest();
            request.AddParameter("filter", "/BindingDescription");
            request.AddParameter("select", "*");
            request.AddParameter("sort", "BadSortValue");

            // Act
            var restResponse = _client.Execute(request);

            // Assert
            restResponse.Content.Should()
                .StartWith("sort must be a comma separated list", because: "the sort value was not properly formatted");
        }

        [Fact]
        public void T005_It_can_get_a_resource_by_its_ObjectID()
        {
            // Arrange
            var objTypeRequest = new RestRequest($"{ResourceEndpoint}/fb89aefa-5ea1-47f1-8890-abe7797d6497");
            objTypeRequest.AddParameter("select", "DisplayName");

            // Act
            var objType = _client.Execute<IdmResource>(objTypeRequest);

            // Assert
            objType.Data.Should().NotBeNull();
            objType.Data.DisplayName.Should().Be("Built-in Synchronization Account");
        }

        [Fact]
        public void T006_It_can_get_any_or_all_attributes_for_a_resource_by_its_ObjectID()
        {
            // Arrange
            var objTypeRequest = new RestRequest($"{ResourceEndpoint}/fb89aefa-5ea1-47f1-8890-abe7797d6497");
            objTypeRequest.AddParameter("select", "*");

            // Act
            var objTypeResponse = _client.Execute<IdmResource>(objTypeRequest);

            // Assert
            var person = new Person(objTypeResponse.Data);
            person.DisplayName.Should().Be("Built-in Synchronization Account");
            person.MailNickname.Should().Be("ILMSync");
        }

        [Fact]
        public void T007_It_can_return_the_number_of_matching_records_for_a_given_search()
        {
            // Arrange
            var request = new RestRequest(ResourceEndpoint, Method.HEAD);
            request.AddParameter("filter", "/ConstantSpecifier");

            // Act
            var response = _client.Execute(request);

            // Assert
            response.Headers.Should().Contain(h => h.Name == "x-idm-count" && h.Value.ToString() == "97");
        }



        private void AssertBindingAsExpected(IdmResource idmResource, string expectedObjectType, string expectedAttrType)
        {
            var resource = new BindingDescription(idmResource);

            var objTypeRequest = new RestRequest($"{ResourceEndpoint}/{resource.BoundObjectType.ObjectID}");
            objTypeRequest.AddParameter("select", "Name");
            var objType = _client.Execute<IdmResource>(objTypeRequest);
            objType.Data.Should().NotBeNull();
            objType.Data.GetAttr("Name").Value.Should().Be(expectedObjectType);

            var attrTypeRequest = new RestRequest($"{ResourceEndpoint}/{resource.BoundAttributeType.ObjectID}");
            attrTypeRequest.AddParameter("select", "*");
            var attrType = _client.Execute<IdmResource>(attrTypeRequest);
            attrType.Data.Should().NotBeNull();
            attrType.Data.GetAttr("Name").Value.Should().Be(expectedAttrType);
        }

        private static RestRequest BuildGetRequest()
        {
            return new RestRequest(ResourceEndpoint);
        }
    }
}
