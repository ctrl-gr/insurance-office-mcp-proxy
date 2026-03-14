using InsuranceProxy.Interfaces;
using InsuranceProxy.Models;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace InsuranceProxy.Services;

public class ToolRouter : IToolRouter
{
    private readonly ICompanyClientRegistry _registry;
    private readonly ILogger<ToolRouter> _logger;
    private readonly List<ProxyToolRegistration> _toolMap = new();

    public ToolRouter(ICompanyClientRegistry registry, ILogger<ToolRouter> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    public async Task<IReadOnlyList<(string NamespacedName, string Description)>> GetAllToolsAsync(
    CancellationToken ct = default)
    {
        _toolMap.Clear();
        var result = new List<(string, string)>();

        foreach (var companyId in _registry.GetActiveCompanies())
        {
            try
            {
                var client = await _registry.GetOrReconnectAsync(companyId, ct);
                var tools = await client.ListToolsAsync(cancellationToken: ct);

                foreach (var tool in tools)
                {
                    var namespacedName = ToolNamespace.Build(companyId, tool.Name);
                    _toolMap.Add(new ProxyToolRegistration(
                        NamespacedName: namespacedName,
                        OriginalName: tool.Name,
                        CompanyId: companyId,
                        ToolDefinition: tool
                    ));
                    result.Add((namespacedName, tool.Description ?? string.Empty));
                }

                _logger.LogInformation("Loaded {Count} tools from {CompanyId}", tools.Count, companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load tools from {CompanyId}", companyId);
            }
        }

        return result;
    }

    public async Task<string> RouteToolCallAsync(
        string namespacedToolName,
        IDictionary<string, object?> arguments,
        CancellationToken ct = default)
    {
        var (companyId, originalToolName) = ToolNamespace.Parse(namespacedToolName);
        var client = await _registry.GetOrReconnectAsync(companyId, ct);

        _logger.LogInformation("Routing {Tool} → {CompanyId}", namespacedToolName, companyId);

        var result = await client.CallToolAsync(
            originalToolName,
            new Dictionary<string, object?>(arguments),
            cancellationToken: ct);

        return JsonSerializer.Serialize(result.Content);
    }
    public string ExtractCompanyId(string namespacedToolName)
        => ToolNamespace.Parse(namespacedToolName).companyId;
}