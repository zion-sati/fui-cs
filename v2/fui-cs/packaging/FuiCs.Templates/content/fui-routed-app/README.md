# FuiRoutedApp

A routed FuiCs application. Each route is an application of its own; one shell
loads the route the address names and swaps routes in place as links are
followed. Publish it and serve the static site:

```bash
dotnet publish -c Release -o site
```

Serve `site/` at the root of any static file server and open its address.

- `FuiRoutedApp.csproj` is the home route and the site: publishing it publishes
  every route listed in its `FuiCsRoute` items into the folder the route's path
  names, with a copy of the page so each address loads directly.
- `Routes/Settings/` is the settings route, at `/settings/`.
- `Shared/` holds code the routes share: the route paths and the page layout
  with its links. JavaScript the routes call goes here too, as a module the
  shared project declares; see
  [host services and events](https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/HOST_SERVICES_AND_WORKERS.md).

To add a route, create a project under `Routes/` like the settings route, list
it as a `FuiCsRoute` in `FuiRoutedApp.csproj`, and add its path to
`Shared/Site.cs`. To serve the site below the root, set `FuiCsRouteBase`
(for example `/app/`) and prefix the paths in `Shared/Site.cs` with it.

The FuiCs guides start at
<https://github.com/zion-sati/fui-cs/blob/main/docs/v2/fui-cs/SDK_INDEX.md>.
