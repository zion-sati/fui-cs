// The harness a packaged FuiCs application publishes with: it runs the application module the page's fui-app.js
// names (window.__fuiCsApp, written at publish) as the page's Tier 3 app. Routed applications publish with
// routed-app-harness.ts; applications with host services or events write a harness of their own, as the demo does.

import { startHarness, type HarnessExports, type HarnessState } from '@effindomv2/runtime/managed-harness';

import { createNetWasmAppInstantiator, NETWASM_TIER3_WASM_PATH, type NetWasmApp } from './tier3-adapter.ts';

interface FuiCsAppConfig {
  /** The application's NetWasm module, relative to the page. */
  readonly module: string;
}

declare global {
  interface Window {
    __fuiCsApp?: FuiCsAppConfig;
    __fuiReady?: boolean;
    __fuiError?: string;
    __fuiState?: HarnessState;
  }
}

interface AppExports extends HarnessExports {
  __runApp(this: void): void;
}

async function openAppModule(): Promise<NetWasmApp> {
  const modulePath = window.__fuiCsApp?.module;
  if (modulePath === undefined) {
    throw new Error('fui-app.js did not name the application module.');
  }
  const module: unknown = await import(new URL(modulePath, document.baseURI).href);
  const open: unknown = typeof module === 'object' && module !== null ? (module as Record<string, unknown>).open : undefined;
  if (typeof open !== 'function') {
    throw new Error(`${modulePath} does not export open().`);
  }
  return await (open as () => Promise<NetWasmApp>)();
}

startHarness<AppExports>({
  wasmPath: NETWASM_TIER3_WASM_PATH,
  instantiateApp: createNetWasmAppInstantiator(openAppModule),
  run(exports): void {
    exports.__runApp();
  },
  onStateUpdated(state): void {
    window.__fuiState = state;
  },
  async onReady({ waitForFrame }): Promise<void> {
    await waitForFrame();
    window.__fuiReady = true;
    delete window.__fuiError;
  },
  onError(error): void {
    window.__fuiError = error instanceof Error ? error.message : String(error);
    console.error(error);
  },
});
