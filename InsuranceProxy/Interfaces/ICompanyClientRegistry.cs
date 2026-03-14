using InsuranceProxy.Models;
using ModelContextProtocol.Client;

namespace InsuranceProxy.Interfaces;

public interface ICompanyClientRegistry
{
    Task RegisterAsync(string companyId, string serverUrl, CancellationToken ct = default);
    Task RegisterAsync(CompanyServerConfig config, CancellationToken ct = default);
    Task UnregisterAsync(string companyId, CancellationToken ct = default);
    McpClient GetClient(string companyId);
    Task<McpClient> GetOrReconnectAsync(string companyId, CancellationToken ct = default);
    IReadOnlyList<string> GetActiveCompanies();
}