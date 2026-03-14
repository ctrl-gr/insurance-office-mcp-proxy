using InsuranceProxy.Interfaces;
using InsuranceProxy.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace InsuranceProxy.Tests;

public class ToolRouterTests
{
    private readonly ICompanyClientRegistry _registry;
    private readonly ToolRouter _router;

    public ToolRouterTests()
    {
        _registry = Substitute.For<ICompanyClientRegistry>();
        _router = new ToolRouter(_registry, NullLogger<ToolRouter>.Instance);
    }

    // ── ExtractCompanyId ───────────────────────────────────
    [Fact]
    public void ExtractCompanyId_ValidNamespacedName_ReturnsCompanyId()
    {
        var companyId = _router.ExtractCompanyId("thelion__get_quote");
        Assert.Equal("thelion", companyId);
    }

    [Fact]
    public void ExtractCompanyId_CompanyA_ReturnsCompanyA()
    {
        var companyId = _router.ExtractCompanyId("companya__echo");
        Assert.Equal("companya", companyId);
    }

    [Fact]
    public void ExtractCompanyId_NoPrefix_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _router.ExtractCompanyId("get_quote"));
    }

    // ── GetAllToolsAsync ───────────────────────────────────
    [Fact]
    public async Task GetAllTools_NoActiveCompanies_ReturnsEmptyList()
    {
        _registry.GetActiveCompanies().Returns(new List<string>());

        var tools = await _router.GetAllToolsAsync();

        Assert.Empty(tools);
    }

    [Fact]
    public async Task GetAllTools_ClientThrows_SkipsCompanyAndContinues()
    {
        _registry.GetActiveCompanies().Returns(new List<string> { "thelion", "companya" });
        _registry.GetClient("thelion").Throws(new Exception("Server unreachable"));
        _registry.GetClient("companya").Throws(new Exception("Server unreachable"));

        // Should not throw — just return empty
        var tools = await _router.GetAllToolsAsync();
        Assert.Empty(tools);
    }

    // ── RouteToolCallAsync ─────────────────────────────────
    [Fact]
    public async Task RouteToolCall_UnknownCompany_ThrowsKeyNotFoundException()
    {
        _registry.GetClient("unknown").Throws(new KeyNotFoundException("No client for 'unknown'"));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _router.RouteToolCallAsync(
                "unknown__get_quote",
                new Dictionary<string, object?>(),
                CancellationToken.None));
    }
    [Fact]
    public async Task RouteToolCall_NoPrefix_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _router.RouteToolCallAsync(
                "get_quote",
                new Dictionary<string, object?>(),
                CancellationToken.None));
    }
}