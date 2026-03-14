using InsuranceProxy.Services;

namespace InsuranceProxy.Tests;

public class ToolNamespaceTests
{
    // ── Build ──────────────────────────────────────────────
    [Fact]
    public void Build_ValidInputs_ReturnsNamespacedName()
    {
        var result = ToolNamespace.Build("thelion", "get_quote");
        Assert.Equal("thelion__get_quote", result);
    }

    [Fact]
    public void Build_UppercaseCompanyId_ReturnsLowercase()
    {
        var result = ToolNamespace.Build("TheLion", "get_quote");
        Assert.Equal("thelion__get_quote", result);
    }

    [Fact]
    public void Build_MultipleTools_AllNamespaced()
    {
        Assert.Equal("companya__echo", ToolNamespace.Build("companya", "echo"));
        Assert.Equal("thelion__get_quote", ToolNamespace.Build("thelion", "get_quote"));
        Assert.Equal("thelion__check_coverage", ToolNamespace.Build("thelion", "check_coverage"));
    }

    // ── Parse ──────────────────────────────────────────────
    [Fact]
    public void Parse_ValidNamespacedName_ReturnsCorrectTuple()
    {
        var (companyId, toolName) = ToolNamespace.Parse("thelion__get_quote");
        Assert.Equal("thelion", companyId);
        Assert.Equal("get_quote", toolName);
    }

    [Fact]
    public void Parse_EchoTool_ReturnsCorrectTuple()
    {
        var (companyId, toolName) = ToolNamespace.Parse("companya__echo");
        Assert.Equal("companya", companyId);
        Assert.Equal("echo", toolName);
    }

    [Fact]
    public void Parse_ToolNameWithUnderscore_ParsesCorrectly()
    {
        // tool name itself contains underscore — should still parse correctly
        var (companyId, toolName) = ToolNamespace.Parse("thelion__check_coverage");
        Assert.Equal("thelion", companyId);
        Assert.Equal("check_coverage", toolName);
    }

    [Fact]
    public void Parse_NoPrefix_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ToolNamespace.Parse("get_quote"));
    }

    [Fact]
    public void Parse_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ToolNamespace.Parse(""));
    }

    // ── IsNamespaced ───────────────────────────────────────
    [Fact]
    public void IsNamespaced_NamespacedName_ReturnsTrue()
    {
        Assert.True(ToolNamespace.IsNamespaced("thelion__get_quote"));
    }

    [Fact]
    public void IsNamespaced_PlainName_ReturnsFalse()
    {
        Assert.False(ToolNamespace.IsNamespaced("get_quote"));
    }

    [Fact]
    public void IsNamespaced_EmptyString_ReturnsFalse()
    {
        Assert.False(ToolNamespace.IsNamespaced(""));
    }
}