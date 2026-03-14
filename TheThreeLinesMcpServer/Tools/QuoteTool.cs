using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TheThreeLinesMcpServer.Models;

namespace TheThreeLinesMcpServer.Tools;

[McpServerToolType]
public class QuoteTool
{
    [McpServerTool]
    [Description("Get an insurance quote from The Three Lines Insurance")]
    public static string GetQuote(
        [Description("Client age (18-99)")] int clientAge,
        [Description("Coverage type: auto | home | life")] string coverageType,
        [Description("Asset value in EUR")] decimal assetValue)
    {
        if (clientAge < 18 || clientAge > 99)
            throw new ArgumentOutOfRangeException(nameof(clientAge), "Age must be between 18 and 99");

        var validTypes = new[] { "auto", "home", "life" };
        if (!validTypes.Contains(coverageType.ToLower()))
            throw new ArgumentException($"Coverage type must be one of: {string.Join(", ", validTypes)}");

        if (assetValue <= 0)
            throw new ArgumentException("Asset value must be greater than 0");

        var result = coverageType.ToLower() switch
        {
            "auto" => CalculateAutoQuote(clientAge, assetValue),
            "home" => CalculateHomeQuote(clientAge, assetValue),
            "life" => CalculateLifeQuote(clientAge, assetValue),
            _ => throw new ArgumentException("Invalid coverage type")
        };

        return JsonSerializer.Serialize(result);
    }

    [McpServerTool]
    [Description("Check what is covered under a specific The Three Lines Insurance policy type")]
    public static string CheckCoverage(
        [Description("Coverage type: auto | home | life")] string coverageType)
    {
        var coverage = coverageType.ToLower() switch
        {
            "auto" => new
            {
                CompanyName = "The Three Lines Insurance",
                CoverageType = "auto",
                Included = new[] {
                    "Third party liability up to €25,000,000",
                    "Theft and fire",
                    "Natural events",
                    "Windshield coverage",
                    "24h roadside assistance",
                    "Replacement car up to 60 days",
                    "Legal protection",
                    "Personal accident coverage"
                },
                NotIncluded = new[] {
                    "Racing events"
                },
                Deductible = "€150 per claim"
            },
            "home" => new
            {
                CompanyName = "The Three Lines Insurance",
                CoverageType = "home",
                Included = new[] {
                    "Fire and explosion",
                    "Theft and vandalism",
                    "Water damage",
                    "Natural disasters",
                    "Third party liability €5,000,000",
                    "Temporary accommodation up to €15,000",
                    "Valuables and jewelry up to €10,000",
                    "Smart home devices coverage"
                },
                NotIncluded = new[] {
                    "War and terrorism"
                },
                Deductible = "€100 per claim"
            },
            "life" => new
            {
                CompanyName = "The Three Lines Insurance",
                CoverageType = "life",
                Included = new[] {
                    "Death benefit",
                    "Total permanent disability",
                    "Critical illness (60 conditions)",
                    "Accidental death triple benefit",
                    "Hospitalization allowance",
                    "Rehabilitation coverage",
                    "Mental health support"
                },
                NotIncluded = new[] {
                    "Self-inflicted injuries"
                },
                Deductible = "No deductible"
            },
            _ => throw new ArgumentException($"Coverage type must be one of: auto, home, life")
        };

        return JsonSerializer.Serialize(coverage);
    }

    private static QuoteResult CalculateAutoQuote(int age, decimal vehicleValue)
    {
        // Premium tier — 5.5% base rate
        var baseRate = 0.055m;
        var ageFactor = age < 25 ? 1.3m : age > 65 ? 1.1m : 1.0m;
        var annual = Math.Round(vehicleValue * baseRate * ageFactor, 2);

        return new QuoteResult(
            CompanyName: "The Three Lines Insurance",
            CoverageType: "auto",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Third party liability", "Theft", "Fire", "Natural events", "Roadside assistance", "Replacement car 60d", "Legal protection", "Personal accident"],
            Notes: age < 25 ? "Young driver surcharge (+30%)" :
                   age > 65 ? "Senior driver surcharge (+10%)" : "Premium rate — full coverage"
        );
    }

    private static QuoteResult CalculateHomeQuote(int age, decimal propertyValue)
    {
        // Premium tier — 0.28%
        var baseRate = 0.0028m;
        var annual = Math.Round(propertyValue * baseRate, 2);

        return new QuoteResult(
            CompanyName: "The Three Lines Insurance",
            CoverageType: "home",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Fire", "Theft", "Water damage", "Natural disasters", "Liability €5M", "Temporary accommodation", "Valuables", "Smart home"],
            Notes: "Premium home insurance — lowest deductible on market"
        );
    }

    private static QuoteResult CalculateLifeQuote(int age, decimal coverageAmount)
    {
        // Premium tier — most comprehensive
        var baseRate = age < 40 ? 0.0045m : age < 60 ? 0.011m : 0.024m;
        var annual = Math.Round(coverageAmount * baseRate, 2);

        return new QuoteResult(
            CompanyName: "The Three Lines Insurance",
            CoverageType: "life",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Death benefit", "Total disability", "Critical illness (60 conditions)", "Accidental death x3", "Hospitalization", "Rehabilitation", "Mental health"],
            Notes: age < 40 ? "Preferred rate (under 40)" :
                   age < 60 ? "Standard rate (40-59)" : "Senior rate (60+)"
        );
    }
}