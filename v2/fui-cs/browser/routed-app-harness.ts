// The harness a packaged routed FuiCs application publishes with: it runs the routes the page's fui-app.js lists
// (window.__fuiCsRoutes, written at publish). Each route is a separately compiled application, and the harness swaps
// the route's application in as the address changes.

import type { HarnessExports, HarnessState } from '@effindomv2/runtime/managed-harness';
import { startRoutedHarness, type RoutedHarnessRoute } from '@effindomv2/runtime/routed-harness';

import type { FuiCsRoute, FuiCsRoutes } from './fui-app-routes.ts';
import { createNetWasmAppInstantiator, NETWASM_TIER3_WASM_PATH, netWasmLoadingDetail, type NetWasmApp } from './tier3-adapter.ts';

declare global {
  interface Window {
    __fuiReady?: boolean;
    __fuiError?: string;
    __fuiState?: HarnessState;
  }
}

interface RouteExports extends HarnessExports {
  __runApp(this: void): void;
  __disposeApp?(this: void): void;
}

type Route = RoutedHarnessRoute & FuiCsRoute;

/** The route the harness is loading, set before it instantiates the route's application. */
let loadingRoute: Route | null = null;

async function openRouteModule(): Promise<NetWasmApp> {
  const route = loadingRoute;
  if (route === null) {
    throw new Error('No route is loading.');
  }
  const module: unknown = await import(new URL(route.modulePath, window.location.origin).href);
  const open: unknown = typeof module === 'object' && module !== null ? (module as Record<string, unknown>).open : undefined;
  if (typeof open !== 'function') {
    throw new Error(`${route.modulePath} does not export open().`);
  }
  return await (open as () => Promise<NetWasmApp>)();
}

function startRoutes(config: FuiCsRoutes): void {
  // The harness compiles wasmPath before it instantiates; a NetWasm route has no raw Tier 3 wasm.
  const routes: readonly Route[] = config.routes.map((route) => ({
    ...route,
    wasmPath: NETWASM_TIER3_WASM_PATH,
    loadingDetail: netWasmLoadingDetail(route.modulePath),
  }));
  startRoutedHarness<RouteExports, Route>({
    shellId: 'fui-cs-routes',
    routeBase: config.routeBase,
    routes,
    recreateRuntimeOnWarmRouteSwap: true,
    instantiateApp: createNetWasmAppInstantiator(openRouteModule),
    onRouteLoading(route): void {
      loadingRoute = route;
      window.__fuiReady = false;
    },
    onRouteReady(): void {
      window.__fuiReady = true;
      delete window.__fuiError;
    },
    onHarnessStateUpdated(state): void {
      window.__fuiState = state;
    },
    onHarnessError(error): void {
      window.__fuiError = error instanceof Error ? error.message : String(error);
      console.error(error);
    },
    run(exports): void {
      exports.__runApp();
    },
    onDispose(exports): void {
      exports.__disposeApp?.();
    },
  });
}

window.__fuiReady = false;
const config = window.__fuiCsRoutes;
if (config === undefined || config.routes.length === 0) {
  window.__fuiError = 'fui-app.js did not list the application routes.';
  console.error(window.__fuiError);
} else {
  startRoutes(config);
}
