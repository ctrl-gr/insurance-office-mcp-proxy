namespace TheBlueCompanyMcpServer.Models;

public record QuoteResult(
    string CompanyName,
    string CoverageType,
    decimal AnnualPremium,
    decimal MonthlyPremium,
    string[] Coverages,
    string Notes
);