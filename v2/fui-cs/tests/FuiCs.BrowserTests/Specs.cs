using System.Diagnostics;
using System.Text;

namespace FuiCs.BrowserTests;

/// <summary>Each Playwright spec file in <c>v2/fui-cs/tests</c>, run in Chromium as one test.</summary>
public sealed class Specs
{
    private static readonly string PackageDirectory = FindPackageDirectory();

    public static IEnumerable<string> SpecFiles() =>
        Directory.GetFiles(Path.Combine(PackageDirectory, "tests"), "*.spec.ts")
            .Select(path => Path.GetFileName(path))
            .Order(StringComparer.Ordinal);

    [Test]
    [MethodDataSource(nameof(SpecFiles))]
    public async Task Spec(string file)
    {
        var output = new StringBuilder();
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(OperatingSystem.IsWindows() ? "npx.cmd" : "npx")
            {
                ArgumentList = { "--no-install", "playwright", "test", "-c", "playwright.config.ts", $"tests/{file}", "--reporter=line" },
                WorkingDirectory = PackageDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };
        process.OutputDataReceived += (_, line) => Append(output, line.Data);
        process.ErrorDataReceived += (_, line) => Append(output, line.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            Assert.Fail($"{file} failed (exit code {process.ExitCode}):{Environment.NewLine}{output}");
        }
    }

    private static void Append(StringBuilder output, string? line)
    {
        if (line is null)
        {
            return;
        }
        lock (output)
        {
            output.AppendLine(line);
        }
    }

    /// <summary>v2/fui-cs, the folder that holds FuiCs.slnx, above this assembly's folder.</summary>
    private static string FindPackageDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "FuiCs.slnx")))
            {
                return directory.FullName;
            }
        }
        throw new InvalidOperationException($"No FuiCs.slnx above {AppContext.BaseDirectory}.");
    }
}
