namespace TheLionMcpServer.Models;

public record QuoteRequest(
    int ClientAge,
    string CoverageType,
    decimal VehicleValue
);