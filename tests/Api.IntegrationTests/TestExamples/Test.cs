using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit.Abstractions;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Api.IntegrationTests.TestExamples;

public class Test(ITestOutputHelper output)
{
    [Fact]
    internal async Task This_is_test_example()
    {
        var client = GetClientWithMockedExternalServices();

        var response = await client.GetAsync("/hello");
        var content = await response.Content.ReadAsStringAsync();
        output.WriteLine(content);
        Assert.True(response.IsSuccessStatusCode);
    }

    private static HttpClient GetClientWithMockedExternalServices()
    {
        var externalService = WireMockServer.Start();

        var fakeBody = new
        {
            Message = "Hello"
        };

        var fakeResponse = new
        {
            Message = "Hello",
        };

        // POST
        externalService
            .Given(Request.Create()
                .WithPath("/path1")
                .UsingPost()
                .WithBody(JsonConvert.SerializeObject(fakeBody)))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(JsonConvert.SerializeObject(fakeResponse)));
        
        // GET
        externalService
            .Given(Request.Create().WithPath("/hello").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(JsonConvert.SerializeObject(fakeResponse)));

        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ExternalServiceSettings:Service1:BaseUrl"] = externalService.Url
                });
            });
        }).CreateClient();
    }
}
