using System;
using System.IO;

namespace InsuranceOfficeApi.Schemas;

public static class SchemaLoader
{
    public static string LoadSchemaFile(string fileName)
    {
        var candidates = new[] {
            Path.Combine(Directory.GetCurrentDirectory(), "Schemas", fileName),
            Path.Combine(AppContext.BaseDirectory, "Schemas", fileName),
            Path.Combine(AppContext.BaseDirectory, "..", "Schemas", fileName)
        };

        foreach (var c in candidates)
        {
            if (File.Exists(c))
            {
                return File.ReadAllText(c);
            }
        }

        throw new FileNotFoundException($"Schema file '{fileName}' not found. Searched: {string.Join("; ", candidates)}");
    }
}
