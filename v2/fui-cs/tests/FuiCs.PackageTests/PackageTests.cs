using System.Diagnostics;
using System.Text;

namespace FuiCs.PackageTests;

/// <summary>
/// Applications made from the fui-cs templates, built against freshly packed packages only: the packages' local version,
/// or the version FUICS_PACKAGE_VERSION names (a release rehearsal's).
/// </summary>
public sealed class PackageTests
{
    private static readonly string PackageDirectory = FindPackageDirectory();
    private static readonly string Packages = Path.Combine(PackageDirectory, "build", "packages");
    private static readonly string WorkDirectory = Path.Combine(Path.GetTempPath(), "fuics-package-test-" + Guid.NewGuid().ToString("N"));
    private static readonly string[] PackageIds = ["FuiCs", "FuiCs.Worker", "FuiCs.Runtime", "FuiCs.Templates"];

    [Before(Class)]
    public static async Task PackAndInstallTemplates()
    {
        if (Directory.Exists(Packages))
        {
            Directory.Delete(Packages, recursive: true);
        }
        var version = Environment.GetEnvironmentVariable("FUICS_PACKAGE_VERSION");
        string[] pack = string.IsNullOrEmpty(version)
            ? ["pack", "FuiCs.slnx", "-c", "Release", "-nologo", "-v", "q"]
            : ["pack", "FuiCs.slnx", "-c", "Release", "-nologo", "-v", "q", $"-p:Version={version}"];
        await Run("dotnet", PackageDirectory, pack);
        await ForgetCachedPackages();
        var feed = Directory.CreateDirectory(Path.Combine(WorkDirectory, "feed")).FullName;
        foreach (var package in Directory.GetFiles(Packages, "*.nupkg"))
        {
            File.Copy(package, Path.Combine(feed, Path.GetFileName(package)));
        }
        await File.WriteAllTextAsync(Path.Combine(WorkDirectory, "nuget.config"), $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="fui-cs" value="{feed}" />
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
              </packageSources>
            </configuration>
            """);
        var templates = Directory.GetFiles(feed, "FuiCs.Templates.*.nupkg").Single();
        await Run("dotnet", WorkDirectory, "new", "install", templates, "--debug:custom-hive", Hive);
    }

    [After(Class)]
    public static async Task CleanUp()
    {
        await ForgetCachedPackages();
        if (Directory.Exists(WorkDirectory))
        {
            Directory.Delete(WorkDirectory, recursive: true);
        }
    }

    [Test]
    public async Task OnePageApplication() => await CheckTemplate("fui-app", "TemplateApp", "FUICS_PACKAGE_SITE", "TemplateApp.spec.ts");

    [Test]
    [DependsOn(nameof(OnePageApplication))]
    public async Task RoutedApplication() => await CheckTemplate("fui-routed-app", "RoutedTemplateApp", "FUICS_ROUTED_PACKAGE_SITE", "RoutedTemplateApp.spec.ts");

    /// <summary>
    /// A routed application whose shared project declares a JavaScript module of its own, as a user would add one:
    /// both routes call it through [JSImport] and end their subscriptions when they unload.
    /// </summary>
    [Test]
    [DependsOn(nameof(RoutedApplication))]
    public async Task ApplicationCallingThePagesJavaScript() =>
        await CheckTemplate("fui-routed-app", "ScriptedTemplateApp", "FUICS_PAGE_JAVASCRIPT_SITE", "PageJavaScriptApp.spec.ts", AddPageJavaScript);

    private static string Hive => Path.Combine(WorkDirectory, "hive");

    /// <summary>
    /// Packs reuse their versions (a -local one, or a rehearsal's), so NuGet would restore an earlier pack from its
    /// cache: the cached copies of the versions just packed, and only those, are removed before the restore. They are
    /// removed again afterwards, since a rehearsal's version is the one nuget.org will serve, and a later restore on
    /// this machine would otherwise take the local pack for it.
    /// </summary>
    private static async Task ForgetCachedPackages()
    {
        var listing = await Output("dotnet", PackageDirectory, "nuget", "locals", "global-packages", "--list");
        var cache = listing.Trim()["global-packages:".Length..].Trim();
        foreach (var id in PackageIds)
        {
            foreach (var file in Directory.GetFiles(Packages, id + ".*.nupkg"))
            {
                // FuiCs.*.nupkg also matches the other packages; a version starts with a digit.
                var version = Path.GetFileNameWithoutExtension(file)[(id.Length + 1)..];
                if (!char.IsAsciiDigit(version[0]))
                {
                    continue;
                }
                var cached = Path.Combine(cache, id.ToLowerInvariant(), version.ToLowerInvariant());
                if (Directory.Exists(cached))
                {
                    Directory.Delete(cached, recursive: true);
                }
            }
        }
    }

    private static async Task CheckTemplate(string template, string name, string siteVariable, string spec, Action<string, string>? customize = null)
    {
        await Run("dotnet", WorkDirectory, "new", template, "-o", name, "--debug:custom-hive", Hive);
        var application = Path.Combine(WorkDirectory, name);
        customize?.Invoke(application, name);
        // Each pack reuses its version and NuGet extracts every file with the package's fixed timestamp, so a compiler
        // server that compiled against an earlier pack would keep that pack's FuiCs.dll.
        await Run("dotnet", application, "publish", "-c", "Release", "-o", "site", "-nologo", "-v", "q", "-p:UseSharedCompilation=false");
        _ = await Run(OperatingSystem.IsWindows() ? "npx.cmd" : "npx", PackageDirectory,
            new Dictionary<string, string> { [siteVariable] = Path.Combine(application, "site") },
            "--no-install", "playwright", "test", "-c", "playwright.package.config.ts", $"tests/package/{spec}", "--reporter=line");
    }

    /// <summary>
    /// Gives a routed application a page JavaScript module in its shared project, declared there only, and has each
    /// route show the module's tick and end its subscription when it unloads.
    /// </summary>
    private static void AddPageJavaScript(string application, string name)
    {
        var host = Directory.CreateDirectory(Path.Combine(application, "Shared", "Host")).FullName;
        File.WriteAllText(Path.Combine(host, "shell.mjs"), """
            const shell = (globalThis.fuiShell ??= { tick: 0, listeners: new Set() });
            shell.timer ??= setInterval(() => {
              shell.tick += 1;
              for (const listener of shell.listeners) listener(shell.tick);
            }, 250);

            export function onTick(callback) {
              shell.listeners.add(callback);
              return { dispose() { shell.listeners.delete(callback); } };
            }
            """);
        File.WriteAllText(Path.Combine(host, "Shell.cs"), $$"""
            using System;
            using System.Runtime.InteropServices.JavaScript;

            namespace {{name}}.Shared;

            public static class Shell
            {
                [JSImport("onTick", "app.shell")]
                public static extern JSSubscription OnTick(Action<int> handler);
            }
            """);
        Replace(Path.Combine(application, "Shared", name + ".Shared.csproj"), "  <ItemGroup>\n    <PackageReference",
            "  <ItemGroup>\n    <NetWasmJavaScriptModule Include=\"Host/shell.mjs\" Module=\"app.shell\" />\n  </ItemGroup>\n\n  <ItemGroup>\n    <PackageReference");
        foreach (var (program, route) in new[] { (Path.Combine(application, "FuiProgram.cs"), "Home"), (Path.Combine(application, "Routes", "Settings", "FuiProgram.cs"), "Settings") })
        {
            Replace(program, "    private static Node BuildPage()\n    {",
                "    private static Node BuildPage()\n    {\n        var tick = new Text(\"" + route + " tick 0\");\n" +
                "        var subscription = Shell.OnTick(value => tick.Text = \"" + route + " tick \" + value);\n" +
                "        Application.OnUnmount(subscription.Dispose);");
            Replace(program, "        return Site.Page(\n            \"" + route + "\",", "        return Site.Page(\n            \"" + route + "\",\n            tick,");
        }
    }

    private static void Replace(string path, string oldText, string newText)
    {
        var text = File.ReadAllText(path).ReplaceLineEndings("\n");
        if (!text.Contains(oldText, StringComparison.Ordinal))
        {
            Assert.Fail($"{path} no longer contains the text the package test edits:{Environment.NewLine}{oldText}");
        }
        File.WriteAllText(path, text.Replace(oldText, newText, StringComparison.Ordinal));
    }

    private static async Task Run(string command, string directory, params string[] arguments) =>
        await Run(command, directory, new Dictionary<string, string>(), arguments);

    private static async Task<string> Output(string command, string directory, params string[] arguments) =>
        await Run(command, directory, new Dictionary<string, string>(), arguments);

    /// <summary>Runs a command, returning its output, and fails with the output when the command fails.</summary>
    private static async Task<string> Run(string command, string directory, IReadOnlyDictionary<string, string> environment, params string[] arguments)
    {
        var start = new ProcessStartInfo(command)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }
        // MSBuild worker nodes kept alive for reuse inherit the redirected output, so its end would come only when
        // they time out, 15 minutes later.
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        foreach (var (key, value) in environment)
        {
            start.Environment[key] = value;
        }
        var output = new StringBuilder();
        using var process = new Process { StartInfo = start };
        process.OutputDataReceived += (_, line) => Append(output, line.Data);
        process.ErrorDataReceived += (_, line) => Append(output, line.Data);
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            Assert.Fail($"{command} {string.Join(' ', arguments)} failed (exit code {process.ExitCode}):{Environment.NewLine}{output}");
        }
        return output.ToString();
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
