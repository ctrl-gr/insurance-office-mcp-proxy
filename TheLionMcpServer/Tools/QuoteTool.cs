using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using TheLionMcpServer.Models;

namespace TheLionMcpServer.Tools;

[McpServerToolType]
public class QuoteTool
{
    [McpServerTool]
    [Description("Get an insurance quote from The Lion Insurance Company")]
    public static string GetQuote(
        [Description("Client age (18-99)")] int clientAge,
        [Description("Coverage type: auto | home | life")] string coverageType,
        [Description("Asset value in EUR (vehicle value for auto, property value for home)")] decimal assetValue)
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
    [Description("Check what is covered under a specific The Lion Insurance policy type")]
    public static string CheckCoverage(
        [Description("Coverage type: auto | home | life")] string coverageType)
    {
        var coverage = coverageType.ToLower() switch
        {
            "auto" => new
            {
                CompanyName = "The Lion Insurance",
                CoverageType = "auto",
                Included = new[] {
                    "Third party liability up to €10,000,000",
                    "Theft and fire",
                    "Natural events",
                    "Windshield coverage",
                    "24h roadside assistance"
                },
                NotIncluded = new[] {
                    "Mechanical breakdown",
                    "Wear and tear",
                    "Racing events"
                },
                Deductible = "€500 per claim"
            },
            "home" => new
            {
                CompanyName = "The Lion Insurance",
                CoverageType = "home",
                Included = new[] {
                    "Fire and explosion",
                    "Theft and vandalism",
                    "Water damage",
                    "Natural disasters",
                    "Third party liability €2,000,000"
                },
                NotIncluded = new[] {
                    "Gradual deterioration",
                    "War and terrorism",
                    "Nuclear events"
                },
                Deductible = "€250 per claim"
            },
            "life" => new
            {
                CompanyName = "The Lion Insurance",
                CoverageType = "life",
                Included = new[] {
                    "Death benefit",
                    "Total permanent disability",
                    "Critical illness (36 conditions)",
                    "Accidental death double benefit"
                },
                NotIncluded = new[] {
                    "Pre-existing conditions (first 2 years)",
                    "Self-inflicted injuries",
                    "Extreme sports"
                },
                Deductible = "No deductible"
            },
            _ => throw new ArgumentException($"Coverage type must be one of: auto, home, life")
        };

        return JsonSerializer.Serialize(coverage);
    }

    private static QuoteResult CalculateAutoQuote(int age, decimal vehicleValue)
    {
        var baseRate = 0.045m; // 4.5% of vehicle value
        var ageFactor = age < 25 ? 1.4m : age > 65 ? 1.2m : 1.0m;
        var annual = Math.Round(vehicleValue * baseRate * ageFactor, 2);

        return new QuoteResult(
            CompanyName: "The Lion Insurance",
            CoverageType: "auto",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Third party liability", "Theft", "Fire", "Natural events", "Roadside assistance"],
            Notes: age < 25 ? "Young driver surcharge applied (+40%)" :
                   age > 65 ? "Senior driver surcharge applied (+20%)" : "Standard rate applied"
        );
    }

    private static QuoteResult CalculateHomeQuote(int age, decimal propertyValue)
    {
        var baseRate = 0.002m; // 0.2% of property value
        var annual = Math.Round(propertyValue * baseRate, 2);

        return new QuoteResult(
            CompanyName: "The Lion Insurance",
            CoverageType: "home",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Fire", "Theft", "Water damage", "Natural disasters", "Liability"],
            Notes: "Standard home insurance rate"
        );
    }

    private static QuoteResult CalculateLifeQuote(int age, decimal coverageAmount)
    {
        var baseRate = age < 40 ? 0.003m : age < 60 ? 0.008m : 0.018m;
        var annual = Math.Round(coverageAmount * baseRate, 2);

        return new QuoteResult(
            CompanyName: "The Lion Insurance",
            CoverageType: "life",
            AnnualPremium: annual,
            MonthlyPremium: Math.Round(annual / 12, 2),
            Coverages: ["Death benefit", "Total disability", "Critical illness", "Accidental death"],
            Notes: age < 40 ? "Preferred rate (under 40)" :
                   age < 60 ? "Standard rate (40-59)" : "Senior rate (60+)"
        );
    }
}