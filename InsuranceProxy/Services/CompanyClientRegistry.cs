using InsuranceProxy.Interfaces;
using InsuranceProxy.Models;
using ModelContextProtocol.Client;

namespace InsuranceProxy.Services;

public class CompanyClientRegistry : ICompanyClientRegistry, IAsyncDisposable
{
    private readonly Dictionary<string, (McpClient Client, CompanyServerConfig Config)> _clients = new();
    private readonly JwtTokenService _jwtService;
    private readonly ILogger<CompanyClientRegistry> _logger;

    public CompanyClientRegistry(JwtTokenService jwtService, ILogger<CompanyClientRegistry> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    // Kept for interface compatibility — delegates to config overload
    public Task RegisterAsync(string companyId, string serverUrl, CancellationToken ct = default)
        => throw new NotSupportedException("Use RegisterAsync(CompanyServerConfig) instead");

    public async Task RegisterAsync(CompanyServerConfig config, CancellationToken ct = default)
    {
        if (_clients.ContainsKey(config.CompanyId))
        {
            _logger.LogWarning("Company {CompanyId} already registered, skipping", config.CompanyId);
            return;
        }

        _logger.LogInformation("Registering {CompanyId} at {ServerUrl}", config.CompanyId, config.ServerUrl);
        var client = await CreateClientAsync(config, ct);
        _clients[config.CompanyId] = (client, config);
        _logger.LogInformation("Company {CompanyId} registered successfully", config.CompanyId);
    }

    public async Task UnregisterAsync(string companyId, CancellationToken ct = default)
    {
        if (!_clients.TryGetValue(companyId, out var entry))
            return;

        await entry.Client.DisposeAsync();
        _clients.Remove(companyId);
    }

    public McpClient GetClient(string companyId)
    {
        if (!_clients.TryGetValue(companyId, out var entry))
            throw new KeyNotFoundException($"No client for '{companyId}'");
        return entry.Client;
    }

    public async Task<McpClient> GetOrReconnectAsync(string companyId, CancellationToken ct = default)
    {
        if (!_clients.TryGetValue(companyId, out var entry))
            throw new KeyNotFoundException($"No client for '{companyId}'");

        try
        {
            await entry.Client.PingAsync(cancellationToken: ct);
            return entry.Client;
        }
        catch
        {
            _logger.LogWarning("Session expired for {CompanyId} — reconnecting...", companyId);
            await entry.Client.DisposeAsync();

            var newClient = await CreateClientAsync(entry.Config, ct);
            _clients[companyId] = (newClient, entry.Config);
            _logger.LogInformation("Reconnected to {CompanyId}", companyId);
            return newClient;
        }
    }

    public IReadOnlyList<string> GetActiveCompanies() => _clients.Keys.ToList();

    private async Task<McpClient> CreateClientAsync(CompanyServerConfig config, CancellationToken ct)
    {
        var token = _jwtService.GenerateToken(config.JwtAudience, config.JwtSecretKey);

        var options = new HttpClientTransportOptions
        {
            Endpoint = new Uri(config.ServerUrl),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {token}"
            }
        };

        var transport = new HttpClientTransport(options);
        return await McpClient.CreateAsync(transport, cancellationToken: ct);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var entry in _clients.Values)
            await entry.Client.DisposeAsync();
        _clients.Clear();
    }
}