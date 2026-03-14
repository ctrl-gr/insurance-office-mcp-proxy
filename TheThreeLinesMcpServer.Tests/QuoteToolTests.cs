using System.Text.Json;
using TheThreeLinesMcpServer.Models;
using TheThreeLinesMcpServer.Tools;

namespace TheThreeLinesMcpServer.Tests;

public class QuoteToolTests
{
    // ── GetQuote — Auto ────────────────────────────────────
    [Fact]
    public void GetQuote_AutoStandardAge_ReturnsCorrectPremium()
    {
        var json = QuoteTool.GetQuote(35, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        Assert.Equal("The Three Lines Insurance", result.CompanyName);
        Assert.Equal("auto", result.CoverageType);
        // 25000 * 0.055 * 1.0 = 1375
        Assert.Equal(1375.00m, result.AnnualPremium);
        Assert.Equal(114.58m, result.MonthlyPremium);
    }

    [Fact]
    public void GetQuote_AutoYoungDriver_AppliesSurcharge()
    {
        var json = QuoteTool.GetQuote(22, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 25000 * 0.055 * 1.3 = 1787.50
        Assert.Equal(1787.50m, result.AnnualPremium);
        Assert.Contains("Young driver", result.Notes);
    }

    [Fact]
    public void GetQuote_AutoSeniorDriver_AppliesSurcharge()
    {
        var json = QuoteTool.GetQuote(70, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 25000 * 0.055 * 1.1 = 1512.50
        Assert.Equal(1512.50m, result.AnnualPremium);
        Assert.Contains("Senior driver", result.Notes);
    }

    // ── GetQuote — Home ────────────────────────────────────
    [Fact]
    public void GetQuote_Home_ReturnsCorrectPremium()
    {
        var json = QuoteTool.GetQuote(40, "home", 300000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 300000 * 0.0028 = 840
        Assert.Equal(840.00m, result.AnnualPremium);
        Assert.Equal(70.00m, result.MonthlyPremium);
    }

    // ── GetQuote — Life ────────────────────────────────────
    [Fact]
    public void GetQuote_LifeUnder40_UsesPreferredRate()
    {
        var json = QuoteTool.GetQuote(30, "life", 200000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 200000 * 0.0045 = 900
        Assert.Equal(900.00m, result.AnnualPremium);
        Assert.Contains("Preferred rate", result.Notes);
    }

    [Fact]
    public void GetQuote_LifeOver60_UsesSeniorRate()
    {
        var json = QuoteTool.GetQuote(65, "life", 200000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 200000 * 0.024 = 4800
        Assert.Equal(4800.00m, result.AnnualPremium);
        Assert.Contains("Senior rate", result.Notes);
    }

    // ── Validation ─────────────────────────────────────────
    [Fact]
    public void GetQuote_AgeTooYoung_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuoteTool.GetQuote(17, "auto", 25000));
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

    // ── CheckCoverage ──────────────────────────────────────
    [Fact]
    public void CheckCoverage_Auto_HasHighestLiability()
    {
        var json = QuoteTool.CheckCoverage("auto");
        var doc = JsonDocument.Parse(json).RootElement;

        Assert.Equal("The Three Lines Insurance", doc.GetProperty("CompanyName").GetString());
        var included = doc.GetProperty("Included").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        // Three Lines has €25M — highest on market
        Assert.Contains(included, i => i!.Contains("25,000,000"));
    }

    [Fact]
    public void CheckCoverage_Home_HasLowestDeductible()
    {
        var json = QuoteTool.CheckCoverage("home");
        var doc = JsonDocument.Parse(json).RootElement;

        Assert.Equal("€100 per claim", doc.GetProperty("Deductible").GetString());
    }

    [Fact]
    public void CheckCoverage_Life_Has60Conditions()
    {
        var json = QuoteTool.CheckCoverage("life");
        var doc = JsonDocument.Parse(json).RootElement;

        var included = doc.GetProperty("Included").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        Assert.Contains(included, i => i!.Contains("60"));
    }

    [Fact]
    public void CheckCoverage_Auto_HasLongerReplacementCar()
    {
        var json = QuoteTool.CheckCoverage("auto");
        var doc = JsonDocument.Parse(json).RootElement;

        var included = doc.GetProperty("Included").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        // Three Lines offers 60 days vs Blue Company 30 days
        Assert.Contains(included, i => i!.Contains("60"));
    }

    [Fact]
    public void CheckCoverage_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => QuoteTool.CheckCoverage("boat"));
    }
}