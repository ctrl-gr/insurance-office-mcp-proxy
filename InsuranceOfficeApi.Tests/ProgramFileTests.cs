using System.IO;
using Xunit;

namespace InsuranceOfficeApi.Tests;

public class ProgramFileTests
{
    [Fact]
    public void ProgramContainsToolNamingConvention()
    {
        var baseDir = AppContext.BaseDirectory;
        var programPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "InsuranceOfficeApi", "Program.cs"));
        var content = File.ReadAllText(programPath);
        Assert.Contains("companyid_toolname", content);
    }
}
