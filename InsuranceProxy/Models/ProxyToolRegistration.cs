using ModelContextProtocol.Client;

namespace InsuranceProxy.Models;

public record ProxyToolRegistration(
    string NamespacedName,
    string OriginalName,
    string CompanyId,
    McpClientTool ToolDefinition
);