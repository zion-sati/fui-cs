// The route table a routed FuiCs site's fui-app.js publishes (FuiCs.Web.targets writes it from the FuiCsRoute items).

export interface FuiCsRoute {
  readonly key: string;
  /** The route's address, from the site root, ending in a slash. */
  readonly routePath: string;
  /** The route's NetWasm module, from the site root. */
  readonly modulePath: string;
  readonly title: string;
}

export interface FuiCsRoutes {
  /** The address the site is served at, ending in a slash. */
  readonly routeBase: string;
  readonly routes: readonly FuiCsRoute[];
}

declare global {
  interface Window {
    __fuiCsRoutes?: FuiCsRoutes;
  }
}
