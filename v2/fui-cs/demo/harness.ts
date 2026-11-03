// The demo shell: runs each route's NetWasm application through the routed harness. Every route is a separately
// compiled C# application; the shell opens the module of the route the harness is loading. Publishing the demo's home
// route writes the route table into fui-app.js (window.__fuiCsRoutes) for the address the demo is published at:
// /v2/fui-cs/demo/ in this repository, or a site's root.

import type { HarnessExports, HarnessState } from '@effindomv2/runtime/managed-harness';
import { startRoutedHarness, type RoutedHarnessManagerState, type RoutedHarnessRoute } from '@effindomv2/runtime/routed-harness';

import '../browser/fui-app-routes.ts';
import { createNetWasmAppInstantiator, NETWASM_TIER3_WASM_PATH, netWasmLoadingDetail, type NetWasmApp } from '../browser/tier3-adapter.ts';
import { demoHostEvents } from './host/host-events.ts';
import { demoHostServices } from './host/host-services.ts';
import { startDemoShell } from './host/shell-state.ts';

declare global {
  interface Window {
    __fuiReady?: boolean;
    __fuiError?: string;
    __fuiState?: HarnessState;
    __fuiManagerState?: {
      readonly routePath: string;
      readonly modulePath: string;
      readonly routeLoads: Readonly<Record<string, number>>;
    };
  }
}

interface DemoExports extends HarnessExports {
  __runApp(this: void): void;
  __disposeApp?(this: void): void;
}

interface DemoRoute extends RoutedHarnessRoute {
  readonly key: string;
  readonly modulePath: string;
  readonly title: string;
}

async function loadRoutes(): Promise<{ readonly routeBase: string; readonly routes: readonly DemoRoute[] }> {
  const published = window.__fuiCsRoutes;
  if (published === undefined || published.routes.length === 0) throw new Error('fui-app.js did not list the demo routes.');
  // The harness compiles wasmPath before it instantiates; a NetWasm route has no raw Tier 3 wasm.
  return Promise.resolve({
    routeBase: published.routeBase,
    routes: published.routes.map((route) => ({
      ...route,
      wasmPath: NETWASM_TIER3_WASM_PATH,
      loadingDetail: netWasmLoadingDetail(route.modulePath),
    })),
  });
}

/** The route the harness is loading, set before it instantiates the route's application. */
let loadingRoute: DemoRoute | null = null;

async function openRouteModule(): Promise<NetWasmApp> {
  const route = loadingRoute;
  if (route === null) throw new Error('No demo route is loading.');
  const module: unknown = await import(new URL(route.modulePath, window.location.origin).href);
  const open: unknown = typeof module === 'object' && module !== null ? (module as Record<string, unknown>).open : undefined;
  if (typeof open !== 'function') throw new Error(`${route.modulePath} does not export open().`);
  return await (open as () => Promise<NetWasmApp>)();
}

function publishManagerState(state: RoutedHarnessManagerState, route: DemoRoute): void {
  window.__fuiManagerState = { routePath: route.routePath, modulePath: route.modulePath, routeLoads: { ...state.routeLoads } };
}

window.__fuiReady = false;
startDemoShell();
void loadRoutes()
  .then(({ routeBase, routes }) => {
    startRoutedHarness<DemoExports, DemoRoute>({
      shellId: 'fui-cs-demo-shell',
      routeBase,
      routes,
      recreateRuntimeOnWarmRouteSwap: true,
      devToolsDomMirror: 'on-requested',
      hostServices: demoHostServices,
      hostEvents: demoHostEvents,
      instantiateApp: createNetWasmAppInstantiator(openRouteModule, { hostEvents: demoHostEvents }),
      onRouteLoading(route): void {
        loadingRoute = route;
        window.__fuiReady = false;
      },
      onRouteReady(state, route): void {
        publishManagerState(state, route);
        // The page title is the route's own caption (Application.SetCaption), as in the reference shell.
        window.__fuiReady = true;
      },
      onHarnessStateUpdated(state): void {
        window.__fuiState = state;
      },
      onHarnessError(error): void {
        window.__fuiError = error instanceof Error ? error.message : String(error);
      },
      run(exports): void {
        exports.__runApp();
      },
      onDispose(exports): void {
        exports.__disposeApp?.();
      },
    });
  })
  .catch((error: unknown) => {
    window.__fuiError = error instanceof Error ? error.message : String(error);
  });
