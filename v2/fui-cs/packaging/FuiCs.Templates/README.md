# FuiCs.Templates

`dotnet new` templates for FuiCs applications.

```bash
dotnet new install FuiCs.Templates
dotnet new fui-app -o MyApp
cd MyApp
dotnet publish -c Release -o site
```

| Template | Creates |
|---|---|
| `fui-app` | An application with one page |
| `fui-routed-app` | A shell with two routes, each an application of its own, and a library they share |

The templates need the .NET 11 SDK; the NetWasm SDK and its tools restore
through NuGet on the first build.

Docs: <https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/SDK_INDEX.md>
