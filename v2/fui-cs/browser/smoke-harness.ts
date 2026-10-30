import { startHarness, type HarnessExports, type HarnessState } from '@effindomv2/runtime/managed-harness';

import { installProbeControls, probeHostEvents, probeHostServices } from './probe-host.ts';
import { createNetWasmAppInstantiator, NETWASM_TIER3_WASM_PATH, type NetWasmApp } from './tier3-adapter.ts';

declare global {
  interface Window {
    __fuiReady?: boolean;
    __fuiError?: string;
    __fuiState?: HarnessState;
  }
}

interface SmokeExports extends HarnessExports {
  __runApp(this: void): void;
  __selectTestPage?(this: void, name: string): void;
}

const SMOKE_MODULE_PATH = './browser/FuiCs.Browser.mjs';

async function openSmokeModule(): Promise<NetWasmApp> {
  const module: unknown = await import(new URL(SMOKE_MODULE_PATH, document.baseURI).href);
  const open: unknown = typeof module === 'object' && module !== null ? (module as Record<string, unknown>).open : undefined;
  if (typeof open !== 'function') {
    throw new Error(`${SMOKE_MODULE_PATH} does not export open().`);
  }
  return await (open as () => Promise<NetWasmApp>)();
}

installProbeControls();
startHarness<SmokeExports>({
  wasmPath: NETWASM_TIER3_WASM_PATH,
  hostServices: probeHostServices,
  hostEvents: probeHostEvents,
  instantiateApp: createNetWasmAppInstantiator(openSmokeModule, { hostEvents: probeHostEvents }),
  run(exports): void {
    const testPage = new URLSearchParams(window.location.search).get('page');
    if (testPage !== null) {
      exports.__selectTestPage?.(testPage);
    }
    exports.__runApp();
  },
  onStateUpdated(state): void {
    window.__fuiState = state;
  },
  async onReady({ waitForFrame }): Promise<void> {
    await waitForFrame();
    await waitForFrame();
    window.__fuiReady = true;
    delete window.__fuiError;
  },
  onError(error): void {
    window.__fuiError = error instanceof Error ? error.message : String(error);
  },
});
