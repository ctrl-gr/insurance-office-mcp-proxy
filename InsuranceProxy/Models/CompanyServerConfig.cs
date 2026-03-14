namespace InsuranceProxy.Models;

public record CompanyServerConfig(
    string CompanyId,
    string DisplayName,
    string ServerUrl,
    bool IsEnabled,
    string JwtAudience,
    string JwtSecretKey
);