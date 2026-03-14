using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TheBlueCompanyMcpServer.Models;

namespace TheBlueCompanyMcpServer.Tools;

[McpServerToolType]
public class QuoteTool
{
    [McpServerTool]
    [Description("Get an insurance quote from The Blue Company")]
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
    [Description("Check what is covered under a specific The Blue Company policy type")]
    public static string CheckCoverage(
        [Description("Coverage type: auto | home | life")] string coverageType)
    {
        var coverage = coverageType.ToLower() switch
        {
            "auto" => new
            {
                CompanyName = "The Blue Company",
                CoverageType = "auto",
                Included = new[] {
                    "Third party liability up to €15,000,000",
                    "Theft and fire",
                    "Natural events",
                    "Windshield coverage",
                    "24h roadside assistance",
                    "Replacement car up to 30 days"
                },
                NotIncluded = new[] {
                    "Mechanical breakdown",
                    "Racing events"
                },
                Deductible = "€300 per claim"
            },
            "home" => new
            {
                CompanyName = "The Blue Company",
                CoverageType = "home",
                Included = new[] {
                    "Fire and explosion",
                    "Theft and vandalism",
                    "Water damage",
                    "Natural disasters",
                    "Third party liability €3,000,000",
                    "Temporary accommodation up to €5,000"
                },
                NotIncluded = new[] {
                    "Gradual deterioration",
                    "War and terrorism"
                },
                Deductible = "€200 per claim"
            },
            "life" => new
            {
                CompanyName = "The Blue Company",
                CoverageType = "life",
                Included = new[] {
                    "Death benefit",
                    "Total permanent disability",
                    "Critical illness (48 conditions)",
                    "Accidental death double benefit",
                    "Hospitalization allowance"
                },
                NotIncluded = new[] {
                    "Pre-existing conditions (first year)",
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
        // More competitive on auto — 4% base rate
        var baseRate = 0.040m;
        var ageFactor = age < 25 ? 1.35m : age > 65 ? 1.15m : 1.0m;
        var annual = Math.Round(vehicleValue * baseRate * ageFactor, 2);

        return new QuoteResult(
            CompanyName: "The Blue Company",
            CoverageType: "auto",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Third party liability", "Theft", "Fire", "Natural events", "Roadside assistance", "Replacement car"],
            Notes: age < 25 ? "Young driver surcharge (+35%)" :
                   age > 65 ? "Senior driver surcharge (+15%)" : "Standard rate applied"
        );
    }

    private static QuoteResult CalculateHomeQuote(int age, decimal propertyValue)
    {
        // Slightly more expensive on home — 0.22%
        var baseRate = 0.0022m;
        var annual = Math.Round(propertyValue * baseRate, 2);

        return new QuoteResult(
            CompanyName: "The Blue Company",
            CoverageType: "home",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Fire", "Theft", "Water damage", "Natural disasters", "Liability", "Temporary accommodation"],
            Notes: "Enhanced home insurance with temporary accommodation"
        );
    }

    private static QuoteResult CalculateLifeQuote(int age, decimal coverageAmount)
    {
        // More expensive on life — better coverage (48 conditions)
        var baseRate = age < 40 ? 0.0035m : age < 60 ? 0.009m : 0.020m;
        var annual = Math.Round(coverageAmount * baseRate, 2);

        return new QuoteResult(
            CompanyName: "The Blue Company",
            CoverageType: "life",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Death benefit", "Total disability", "Critical illness (48 conditions)", "Accidental death", "Hospitalization"],
            Notes: age < 40 ? "Preferred rate (under 40)" :
                   age < 60 ? "Standard rate (40-59)" : "Senior rate (60+)"
        );
    }
}