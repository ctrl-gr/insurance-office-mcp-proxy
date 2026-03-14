using System.Text.Json;
using TheBlueCompanyMcpServer.Models;
using TheBlueCompanyMcpServer.Tools;

namespace TheBlueCompanyMcpServer.Tests;

public class QuoteToolTests
{
    // ── GetQuote — Auto ────────────────────────────────────
    [Fact]
    public void GetQuote_AutoStandardAge_ReturnsCorrectPremium()
    {
        var json = QuoteTool.GetQuote(35, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        Assert.Equal("The Blue Company", result.CompanyName);
        Assert.Equal("auto", result.CoverageType);
        // 25000 * 0.040 * 1.0 = 1000
        Assert.Equal(1000.00m, result.AnnualPremium);
        Assert.Equal(83.33m, result.MonthlyPremium);
    }

    [Fact]
    public void GetQuote_AutoYoungDriver_AppliesSurcharge()
    {
        var json = QuoteTool.GetQuote(22, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 25000 * 0.040 * 1.35 = 1350
        Assert.Equal(1350.00m, result.AnnualPremium);
        Assert.Contains("Young driver", result.Notes);
    }

    [Fact]
    public void GetQuote_AutoSeniorDriver_AppliesSurcharge()
    {
        var json = QuoteTool.GetQuote(70, "auto", 25000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 25000 * 0.040 * 1.15 = 1150
        Assert.Equal(1150.00m, result.AnnualPremium);
        Assert.Contains("Senior driver", result.Notes);
    }

    // ── GetQuote — Home ────────────────────────────────────
    [Fact]
    public void GetQuote_Home_ReturnsCorrectPremium()
    {
        var json = QuoteTool.GetQuote(40, "home", 300000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 300000 * 0.0022 = 660
        Assert.Equal(660.00m, result.AnnualPremium);
        Assert.Equal(55.00m, result.MonthlyPremium);
    }

    // ── GetQuote — Life ────────────────────────────────────
    [Fact]
    public void GetQuote_LifeUnder40_UsesPreferredRate()
    {
        var json = QuoteTool.GetQuote(30, "life", 200000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 200000 * 0.0035 = 700
        Assert.Equal(700.00m, result.AnnualPremium);
        Assert.Contains("Preferred rate", result.Notes);
    }

    [Fact]
    public void GetQuote_LifeOver60_UsesSeniorRate()
    {
        var json = QuoteTool.GetQuote(65, "life", 200000);
        var result = JsonSerializer.Deserialize<QuoteResult>(json)!;

        // 200000 * 0.020 = 4000
        Assert.Equal(4000.00m, result.AnnualPremium);
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
    public void CheckCoverage_Auto_HasHigherLiabilityThanLion()
    {
        var json = QuoteTool.CheckCoverage("auto");
        var doc = JsonDocument.Parse(json).RootElement;

        Assert.Equal("The Blue Company", doc.GetProperty("CompanyName").GetString());
        // Blue Company has €15M liability vs TheLion €10M
        var included = doc.GetProperty("Included").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        Assert.Contains(included, i => i!.Contains("15,000,000"));
    }

    [Fact]
    public void CheckCoverage_Home_HasTemporaryAccommodation()
    {
        var json = QuoteTool.CheckCoverage("home");
        var doc = JsonDocument.Parse(json).RootElement;

        var included = doc.GetProperty("Included").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        Assert.Contains(included, i => i!.Contains("accommodation"));
    }

    [Fact]
    public void CheckCoverage_Life_Has48Conditions()
    {
        var json = QuoteTool.CheckCoverage("life");
        var doc = JsonDocument.Parse(json).RootElement;

        var included = doc.GetProperty("Included").EnumerateArray()
            .Select(x => x.GetString()).ToList();
        Assert.Contains(included, i => i!.Contains("48"));
    }

    [Fact]
    public void CheckCoverage_InvalidType_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => QuoteTool.CheckCoverage("boat"));
    }
}