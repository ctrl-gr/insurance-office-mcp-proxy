namespace InsuranceProxy.Services;

public static class ToolNamespace
{
    private const string Separator = "__";

    public static string Build(string companyId, string toolName)
        => $"{companyId.ToLower()}{Separator}{toolName}";

    public static (string companyId, string toolName) Parse(string namespacedName)
    {
        var idx = namespacedName.IndexOf(Separator);
        if (idx < 0)
            throw new ArgumentException(
                $"Tool name '{namespacedName}' has no namespace prefix");

        return (
            namespacedName[..idx],
            namespacedName[(idx + Separator.Length)..]
        );
    }

    public static bool IsNamespaced(string toolName)
        => toolName.Contains(Separator);
}