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
        private const string ResourceEndpoint = "api/resources";
        private const string BaseUrl = "http://localhost:8088/";
        

        public IntegrationTests(IisExpressFixture fixture)
        {
        }

        [Fact]
        public void T001_It_can_search_for_resources_without_specifying_a_select_or_sort()
        {
            // Arrange
            var client = new RestClient(BaseUrl);
            var request = new RestRequest(ResourceEndpoint, Method.GET);
            request.AddParameter("filter", "/ObjectTypeDescription"); 

            // Act
            var restResponse = client.Execute<List<IdmResource>>(request);

            // Assert
            restResponse.Data.Count.Should().BeGreaterOrEqualTo(40);
        }


        [Fact]
        public void It_starts_iisexpress_only_once()
        {

        }
    }
}
