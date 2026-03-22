using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InsuranceOfficeApi.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AppStarts_Returns_404_ForRoot()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
