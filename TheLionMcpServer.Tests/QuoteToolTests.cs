using System.Text.Json;
using TheLionMcpServer.Models;
using TheLionMcpServer.Tools;

namespace TheLionMcpServer.Tests;

public class QuoteToolTests
{
    // ── GetQuote — Auto ────────────────────────────────────
    [Fact]
    public void GetQuote_AutoStandardAge_ReturnsCorrectPremium()
    {
        var json = QuoteTool.GetQuote(35, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        Assert.Equal("The Lion Insurance", result.CompanyName);
        Assert.Equal("auto", result.CoverageType);
        Assert.Equal(1125.00m, result.AnnualPremium);
        Assert.Equal(93.75m, result.MonthlyPremium);
    }

    [Fact]
    public void GetQuote_AutoYoungDriver_AppliesSurcharge()
    {
        var json = QuoteTool.GetQuote(22, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 25000 * 0.045 * 1.4 = 1575
        Assert.Equal(1575.00m, result.AnnualPremium);
        Assert.Contains("Young driver", result.Notes);
    }

    [Fact]
    public void GetQuote_AutoSeniorDriver_AppliesSurcharge()
    {
        var json = QuoteTool.GetQuote(70, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 25000 * 0.045 * 1.2 = 1350
        Assert.Equal(1350.00m, result.AnnualPremium);
        Assert.Contains("Senior driver", result.Notes);
    }

    // ── GetQuote — Home ────────────────────────────────────
    [Fact]
    public void GetQuote_Home_ReturnsCorrectPremium()
    {
        var json = QuoteTool.GetQuote(40, "home", 300000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 300000 * 0.002 = 600
        Assert.Equal(600.00m, result.AnnualPremium);
        Assert.Equal(50.00m, result.MonthlyPremium);
        Assert.Equal("home", result.CoverageType);
    }

    // ── GetQuote — Life ────────────────────────────────────
    [Fact]
    public void GetQuote_LifeUnder40_UsesPreferredRate()
    {
        var json = QuoteTool.GetQuote(30, "life", 200000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 200000 * 0.003 = 600
        Assert.Equal(600.00m, result.AnnualPremium);
        Assert.Contains("Preferred rate", result.Notes);
    }

    [Fact]
    public void GetQuote_LifeOver60_UsesSeniorRate()
    {
        var json = QuoteTool.GetQuote(65, "life", 200000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 200000 * 0.018 = 3600
        Assert.Equal(3600.00m, result.AnnualPremium);
        Assert.Contains("Senior rate", result.Notes);
    }

    // ── GetQuote — Validation ──────────────────────────────
    [Fact]
    public void GetQuote_AgeTooYoung_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuoteTool.GetQuote(17, "auto", 25000));
    }

    [Fact]
    public void GetQuote_AgeTooOld_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuoteTool.GetQuote(100, "auto", 25000));
    }

    [Fact]
    public void GetQuote_InvalidCoverageType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => QuoteTool.GetQuote(35, "boat", 25000));
    }

    [Fact]
    public void GetQuote_ZeroAssetValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => QuoteTool.GetQuote(35, "auto", 0));
    }

    [Fact]
    public void GetQuote_NegativeAssetValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => QuoteTool.GetQuote(35, "auto", -1000));
    }

    // ── CheckCoverage ──────────────────────────────────────
    [Fact]
    public void CheckCoverage_Auto_ContainsExpectedFields()
    {
        var json = QuoteTool.CheckCoverage("auto");
        var doc = JsonDocument.Parse(json).RootElement;

        Assert.Equal("The Lion Insurance", doc.GetProperty("CompanyName").GetString());
        Assert.Equal("auto", doc.GetProperty("CoverageType").GetString());
        Assert.True(doc.GetProperty("Included").GetArrayLength() > 0);
        Assert.True(doc.GetProperty("NotIncluded").GetArrayLength() > 0);
    }

    [Fact]
    public void CheckCoverage_Home_ContainsExpectedFields()
    {
        var json = QuoteTool.CheckCoverage("home");
        var doc = JsonDocument.Parse(json).RootElement;

        Assert.Equal("home", doc.GetProperty("CoverageType").GetString());
        Assert.Equal("€250 per claim", doc.GetProperty("Deductible").GetString());
    }

    [Fact]
    public void CheckCoverage_Life_ContainsExpectedFields()
    {
        var json = QuoteTool.CheckCoverage("life");
        var doc = JsonDocument.Parse(json).RootElement;

        Assert.Equal("No deductible", doc.GetProperty("Deductible").GetString());
    }

    [Fact]
    public void CheckCoverage_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => QuoteTool.CheckCoverage("boat"));
    }

    [Fact]
    public void CheckCoverage_CaseInsensitive_Works()
    {
        var json = QuoteTool.CheckCoverage("AUTO");
        var doc = JsonDocument.Parse(json).RootElement;
        Assert.Equal("auto", doc.GetProperty("CoverageType").GetString());
    }
}