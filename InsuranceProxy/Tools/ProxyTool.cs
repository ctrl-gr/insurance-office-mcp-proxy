using InsuranceProxy.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace InsuranceProxy.Tools;

[McpServerToolType]
public class ProxyTool
{
    private readonly IToolRouter _router;

    public ProxyTool(IToolRouter router)
    {
        _router = router;
    }

    [McpServerTool]
    [Description("List all available tools from all connected insurance company servers")]
    public async Task<string> ListCompanyTools(CancellationToken ct = default)
    {
        var tools = await _router.GetAllToolsAsync(ct);
        var summary = tools.Select(t => new { Name = t.NamespacedName, t.Description });
        return JsonSerializer.Serialize(summary);
    }

    [McpServerTool]
    [Description("Call a tool on a specific insurance company server. Use format: companyid__toolname")]
    public async Task<string> CallCompanyTool(
        [Description("Namespaced tool name, e.g. companya__echo")] string toolName,
        [Description("JSON string of arguments to pass to the tool")] string argumentsJson,
        CancellationToken ct = default)
    {
        var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson)
            ?? new Dictionary<string, object?>();

        return await _router.RouteToolCallAsync(toolName, arguments, ct);
    }
}