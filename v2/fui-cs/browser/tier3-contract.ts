// The contract between the page-side Tier 3 adapter and the fui-cs JSImport modules.
//
// The adapter and the JSImport modules end up in different bundles (the JSImport modules are
// bundled into NetWasm's page runtime), so they meet through one global: the adapter installs
// it before it opens the NetWasm module, and every JSImport call reads it.

import type { AppMemory, OutElement } from './app-memory.ts';

export interface FuiCsTier3Bridge {
  readonly memory: AppMemory;
  readonly imports: WebAssembly.Imports;
}

declare global {
  var __fuiCsTier3: FuiCsTier3Bridge | undefined;
}

export function installTier3Bridge(bridge: FuiCsTier3Bridge): void {
  globalThis.__fuiCsTier3 = bridge;
}

/** Removes `bridge` when it is still the installed one, so a disposed application's memory is not reachable. */
export function uninstallTier3Bridge(bridge: FuiCsTier3Bridge): void {
  if (globalThis.__fuiCsTier3 === bridge) {
    globalThis.__fuiCsTier3 = undefined;
  }
}

export function tier3Bridge(): FuiCsTier3Bridge {
  const bridge = globalThis.__fuiCsTier3;
  if (bridge === undefined) {
    throw new Error('fui-cs Tier 3 bridge is not installed; load the app through the fui-cs NetWasm instantiator.');
  }
  return bridge;
}

type ImportFunction = (...args: unknown[]) => unknown;

export function harnessImport(module: string, name: string): ImportFunction {
  // The harness may omit a module entirely, which the index type does not express.
  const moduleImports = tier3Bridge().imports[module] as WebAssembly.ModuleImports | undefined;
  const value = moduleImports?.[name];
  if (typeof value !== 'function') {
    throw new Error(`The managed harness does not provide ${module}.${name}.`);
  }
  return value as ImportFunction;
}

/**
 * Calls a harness import, releasing every argument-arena allocation made for the call
 * afterwards. `write` lowers the call's values into the app memory and returns the
 * pointer-level argument list.
 */
export function callHarness(module: string, name: string, write: (memory: AppMemory) => readonly unknown[]): unknown {
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    return harnessImport(module, name)(...write(memory));
  } finally {
    memory.release(mark);
  }
}

/**
 * Calls a harness import that writes out values. Each out value gets an 8-byte arena slot;
 * the result is `[return value, ...out values]` as numbers, which the generated C# unpacks.
 */
export function callHarnessWithOutputs(
  module: string,
  name: string,
  outputs: readonly OutElement[],
  write: (memory: AppMemory, outputPtrs: readonly number[]) => readonly unknown[],
): Float64Array {
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    const outputPtrs = outputs.map(() => memory.allocate(8, 8));
    const result = harnessImport(module, name)(...write(memory, outputPtrs));
    const values = new Float64Array(outputs.length + 1);
    values[0] = result === undefined ? 0 : Number(result);
    outputs.forEach((element, index) => {
      values[index + 1] = memory.readScalar(outputPtrs[index] ?? 0, element);
    });
    return values;
  } finally {
    memory.release(mark);
  }
}
