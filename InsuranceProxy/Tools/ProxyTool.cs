using InsuranceProxy.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace InsuranceProxy.Tools;

[McpServerToolType]
public static class ProxyTool
{
    [McpServerTool(Name = "ListCompanyTools")]
    [Description("List all available tools from all connected insurance company servers")]
    public static async Task<string> ListCompanyTools(
        IToolRouter router,
        CancellationToken ct = default)
    {
        var tools = await router.GetAllToolsAsync(ct);
        var summary = tools.Select(t => new { Name = t.NamespacedName, t.Description });
        return JsonSerializer.Serialize(summary);
    }

    [McpServerTool(Name = "CallCompanyTools")]
    [Description("Call a tool on a specific insurance company server. Use format: companyid_toolname")]
    public static async Task<string> CallCompanyTools(
        IToolRouter router,
        [Description("Namespaced tool name, e.g. thelion_get_quote")] string toolName,
        [Description("JSON string of arguments to pass to the tool")] string argumentsJson,
        CancellationToken ct = default)
    {
        var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson)
            ?? new Dictionary<string, object?>();

        return await router.RouteToolCallAsync(toolName, arguments, ct);
    }
}