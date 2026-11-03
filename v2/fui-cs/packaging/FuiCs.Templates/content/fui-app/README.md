# FuiApp

A FuiCs application. Run it, which serves it at `http://localhost:5080/` until you stop it:

```bash
dotnet run
```

To deploy it, publish it and serve the static site:

```bash
dotnet publish -c Release -o site
```

Serve `site/` with any static file server and open its address. The page is
built in `FuiProgram.cs`; the FuiCs guides start at
<https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/SDK_INDEX.md>.
