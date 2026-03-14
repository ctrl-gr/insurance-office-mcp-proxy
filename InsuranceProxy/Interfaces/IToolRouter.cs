namespace InsuranceProxy.Interfaces;

public interface IToolRouter
{
    Task<IReadOnlyList<(string NamespacedName, string Description)>> GetAllToolsAsync(CancellationToken ct = default);
    Task<string> RouteToolCallAsync(
        string namespacedToolName,
        IDictionary<string, object?> arguments,
        CancellationToken ct = default);
    string ExtractCompanyId(string namespacedToolName);
}