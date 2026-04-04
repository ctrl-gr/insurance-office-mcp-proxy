using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Serialization;
using InsuranceOfficeApi.Schemas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors();

async Task<McpClient> CreateProxyClient()
{
    var proxyUrl = app.Configuration["McpProxy:Url"]!;
    var transport = new HttpClientTransport(new HttpClientTransportOptions
    {
        Endpoint = new Uri(proxyUrl)
    });
    return await McpClient.CreateAsync(transport);
}

var quoteSchemaJson = SchemaLoader.LoadSchemaFile("quoteSchema.json");
var coverageSchemaJson = SchemaLoader.LoadSchemaFile("coverageSchema.json");

var quoteSchema = BinaryData.FromString(quoteSchemaJson);
var coverageSchema = BinaryData.FromString(coverageSchemaJson);

async Task<List<CompanyToolInfo>> GetCompanyTools(McpClient client)
{
    var result = await client.CallToolAsync("ListCompanyTools", new Dictionary<string, object?>());
    var text = result.Content.OfType<TextContentBlock>().Select(c => c.Text).FirstOrDefault() ?? "[]";
    return JsonSerializer.Deserialize<List<CompanyToolInfo>>(text) ?? new();
}

app.MapGet("/api/health", async () =>
{
    try
    {
        await using var client = await CreateProxyClient();
        var tools = await GetCompanyTools(client);
        return Results.Ok(new { status = "ok", toolCount = tools.Count, tools = tools.Select(t => t.Name) });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/tools", async () =>
{
    await using var client = await CreateProxyClient();
    var tools = await GetCompanyTools(client);
    return Results.Ok(tools);
});

app.MapPost("/api/chat", async (ConversationRequest request) =>
{
    var apiKey = app.Configuration["OpenAI:ApiKey"]!;
    var model = app.Configuration["OpenAI:Model"] ?? "gpt-4o";

    await using var mcpClient = await CreateProxyClient();

    var companyTools = await GetCompanyTools(mcpClient);

    var openAiTools = new List<ChatTool>();
    var nameMap = new Dictionary<string, string>();

    foreach (var t in companyTools)
    {
        var schema = t.Name.Contains("get_quote") ? quoteSchema : coverageSchema;
        openAiTools.Add(ChatTool.CreateFunctionTool(t.Name, t.Description ?? string.Empty, schema));
        nameMap[t.Name] = t.Name;
    }

    var messages = new List<OpenAI.Chat.ChatMessage>
    {
        new SystemChatMessage("""
            You are a helpful insurance office assistant. You help clients compare
            insurance quotes from multiple companies: The Lion Insurance, The Blue Company,
            and The Three Lines Insurance.

            When a client asks for quotes or coverage info, use the available tools to
            fetch real data and present a clear comparison. Tool names follow the pattern
            companyid_toolname (e.g. thelion_get_quote, thebluecompany_check_coverage).

            When comparing quotes, always call all 3 companies and present results in a
            clear list showing company name, annual premium, monthly premium, and key coverages.
            Always be concise and helpful. Answer in the same language the user writes in.
            """)
    };

    foreach (var msg in request.History)
    {
        if (msg.Role == "user") messages.Add(new UserChatMessage(msg.Content));
        else if (msg.Role == "assistant") messages.Add(new AssistantChatMessage(msg.Content));
    }

    messages.Add(new UserChatMessage(request.Message));

    var openAiClient = new OpenAIClient(apiKey);
    var chatClient = openAiClient.GetChatClient(model);

    var options = new ChatCompletionOptions();
    foreach (var tool in openAiTools) options.Tools.Add(tool);

    while (true)
    {
        var response = await chatClient.CompleteChatAsync(messages, options);

        if (response.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            messages.Add(new AssistantChatMessage(response.Value));

            foreach (var toolCall in response.Value.ToolCalls)
            {
                var args = JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    toolCall.FunctionArguments.ToString()) ?? new();

                string toolResult;
                try
                {
                    var result = await mcpClient.CallToolAsync(
                        "CallCompanyTools",
                        new Dictionary<string, object?>
                        {
                            ["toolName"] = toolCall.FunctionName,
                            ["argumentsJson"] = JsonSerializer.Serialize(args)
                        });

                    toolResult = result.Content
                        .OfType<TextContentBlock>()
                        .Select(c => c.Text)
                        .FirstOrDefault() ?? "{}";
                }
                catch (Exception ex)
                {
                    toolResult = $"{{\"error\": \"{ex.Message}\"}}";
                }

                messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
            }
        }
        else
        {
            var finalText = response.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
            return Results.Ok(new { reply = finalText });
        }
    }
});

app.Run();

public record ConversationMessage(string Role, string Content);
public record ConversationRequest(string Message, List<ConversationMessage> History);
public record CompanyToolInfo(
    [property: JsonPropertyName("Name")] string Name,
    [property: JsonPropertyName("Description")] string? Description);

public partial class Program { }