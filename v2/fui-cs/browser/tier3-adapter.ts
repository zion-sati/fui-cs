// Runs a fui-cs NetWasm JsModule as the managed harness's Tier 3 app.
//
// The managed harness instantiates Tier 3 through a framework-owned hook. This instantiator
// opens the NetWasm module instead, gives the harness an app memory it can exchange pointers
// with, and exposes the C# [JSExport] functions under the harness's export names. Exports that
// carry (pointer, length) pairs are decoded from the app memory into strings or byte arrays, and so
// are the application's host events, from the page's own event definitions.

import {
  listHostEventMethods,
  type HarnessExports,
  type HostEventsDefinition,
  type HostServiceTypeName,
  type WasmAppInstantiator,
} from '@effindomv2/runtime/managed-harness';

import { AppMemory } from './app-memory.ts';
import { installTier3Bridge, uninstallTier3Bridge, type FuiCsTier3Bridge } from './tier3-contract.ts';
import { installNetWasmWorkers } from './workers.ts';

/**
 * The managed harness compiles `wasmPath` before it calls the instantiator. A NetWasm JsModule
 * has no raw Tier 3 wasm, so fui-cs points the harness at an empty module.
 */
export const NETWASM_TIER3_WASM_PATH = 'data:application/wasm;base64,AGFzbQEAAAA=';

/**
 * The loading overlay's text for a NetWasm application module, naming the WebAssembly NetWasm loads beside it; the
 * harness would otherwise name `wasmPath`, the empty module above.
 */
export function netWasmLoadingDetail(modulePath: string): string {
  const file = modulePath.split('/').at(-1) ?? modulePath;
  return `Loading ${file.replace(/\.mjs$/u, '.wasm')}`;
}

export type NetWasmApp = Readonly<Record<string, unknown>>;
export type OpenNetWasmApp = () => Promise<NetWasmApp>;

type ExportFunction = (...args: unknown[]) => unknown;
type ArgKind = 'value' | 'utf8' | 'bytes';

const BUFFER_EXPORTS = new Set(['__fui_key_buffer', '__fui_text_buffer', '__fui_text_buffer_size']);

/** Harness exports whose (pointer, length) pairs fui-cs receives as strings or byte arrays. */
const POINTER_EXPORTS: Readonly<Partial<Record<string, readonly ArgKind[]>>> = {
  __fui_debug_key_event: ['value', 'utf8', 'value'],
  __fui_on_key_event: ['value', 'utf8', 'value'],
  __fui_on_external_drag_event: ['value', 'value', 'value', 'value', 'value', 'bytes'],
  __fui_on_fetch_complete: ['value', 'value', 'value', 'bytes'],
  __fui_on_fetch_error: ['value', 'utf8'],
  __fui_on_file_pick_result: ['value', 'value', 'bytes'],
  __fui_on_file_read_result: ['value', 'value', 'value', 'value', 'bytes'],
  __fui_on_file_save_result: ['value', 'value', 'value', 'bytes'],
  __fui_on_file_writer_created: ['value', 'value', 'bytes'],
  __fui_on_file_write_result: ['value', 'value', 'value', 'value', 'bytes'],
  __fui_on_file_finish_result: ['value', 'value', 'value', 'bytes'],
  __fui_on_file_worker_process_progress: ['value', 'value', 'value', 'bytes'],
  __fui_on_file_worker_process_chunk: ['value', 'value', 'value', 'bytes'],
  __fui_on_file_worker_process_complete: ['value', 'value', 'bytes'],
  __fui_on_file_worker_process_error: ['value', 'value', 'bytes'],
  __fui_on_text_changed: ['value', 'utf8'],
  __fui_on_text_replaced: ['value', 'value', 'value', 'utf8'],
  __fui_on_cross_selection_changed: ['value', 'utf8'],
  __fui_on_route_changed: ['utf8'],
  __fui_on_svg_failed: ['value', 'utf8'],
  __fui_on_texture_failed: ['value', 'utf8'],
  __fui_on_worker_progress: ['value', 'utf8'],
  __fui_on_worker_complete: ['value', 'utf8'],
  __fui_on_worker_error: ['value', 'utf8'],
};

type ExportName = Exclude<keyof HarnessExports, 'memory'>;
type RequiredExportName = {
  [K in ExportName]-?: undefined extends HarnessExports[K] ? never : K;
}[ExportName];
type ExportResult<K extends RequiredExportName> = HarnessExports[K] extends (...args: never[]) => infer R ? R : never;

/**
 * What each required harness export returns until the C# app implements it. Typed against
 * HarnessExports, so a new required harness export fails to compile here instead of being missed;
 * entries for exports an older published runtime does not require yet are allowed.
 */
const REQUIRED_EXPORT_DEFAULTS: { readonly [K in RequiredExportName]: ExportResult<K> } & Readonly<Record<string, unknown>> = {
  __flushRenders: undefined,
  __fui_on_pointer_event_with_metadata: false,
  __fui_on_wheel_event: 0,
  __fui_on_external_drag_event: 0,
  __fui_on_fetch_complete: undefined,
  __fui_on_fetch_error: undefined,
  __fui_on_file_pick_result: undefined,
  __fui_on_file_read_result: undefined,
  __fui_on_file_save_result: undefined,
  __fui_on_file_writer_created: undefined,
  __fui_on_file_write_result: undefined,
  __fui_on_file_finish_result: undefined,
  __fui_on_file_worker_process_progress: undefined,
  __fui_on_file_worker_process_chunk: undefined,
  __fui_on_file_worker_process_complete: undefined,
  __fui_on_file_worker_process_error: undefined,
  __fui_on_context_menu: undefined,
  __fui_hide_active_context_menu: undefined,
  __fui_key_buffer: 0,
  __fui_text_buffer: 0,
  __fui_text_buffer_size: 0,
  __fui_on_focus_changed: undefined,
  __fui_on_text_changed: undefined,
  __fui_on_text_replaced: undefined,
  __fui_on_selection_changed: undefined,
  __fui_on_key_event: 0,
  __fui_on_scroll: undefined,
  __fui_on_cross_selection_changed: undefined,
  __fui_on_route_changed: undefined,
  __fui_on_viewport_changed: undefined,
  __fui_on_system_dark_mode_changed: undefined,
  __fui_on_system_accent_color_changed: undefined,
  __fui_on_svg_loaded: undefined,
  __fui_on_svg_failed: undefined,
  __fui_on_texture_loaded: undefined,
  __fui_on_texture_failed: undefined,
  __fui_on_frame: undefined,
  __fui_needs_animation_frame: false,
  __fui_on_timer: undefined,
  __fui_on_font_loaded: undefined,
  __fui_on_worker_progress: undefined,
  __fui_on_worker_complete: undefined,
  __fui_on_worker_error: undefined,
};

function decodeArguments(memory: AppMemory, kinds: readonly ArgKind[], args: readonly unknown[]): unknown[] {
  const decoded: unknown[] = [];
  let index = 0;
  for (const kind of kinds) {
    if (kind === 'value') {
      decoded.push(args[index]);
      index += 1;
      continue;
    }
    const ptr = Number(args[index]);
    const len = Number(args[index + 1]);
    decoded.push(kind === 'utf8' ? memory.readUtf8(ptr, len) : memory.readBytes(ptr, len));
    index += 2;
  }
  return decoded;
}

const ARRAY_ELEMENT_BYTES: Readonly<Partial<Record<HostServiceTypeName, number>>> = {
  i32_array: 4,
  u32_array: 4,
  i64_array: 8,
  u64_array: 8,
  f64_array: 8,
};

function liftArray(type: HostServiceTypeName, bytes: Uint8Array): unknown {
  switch (type) {
    case 'i32_array': return new Int32Array(bytes.buffer);
    case 'u32_array': return new Uint32Array(bytes.buffer);
    case 'i64_array': return new BigInt64Array(bytes.buffer);
    case 'u64_array': return new BigUint64Array(bytes.buffer);
    default: return new Float64Array(bytes.buffer);
  }
}

/**
 * Lifts a host event's arguments back into the values the C# export takes. The harness passes them as to any
 * Tier 3 app: strings and arrays as (pointer, length) pairs in the text buffer, where an array's length counts
 * elements, and booleans as 0 or 1.
 */
function decodeHostEventArguments(memory: AppMemory, types: readonly HostServiceTypeName[], args: readonly unknown[]): unknown[] {
  const decoded: unknown[] = [];
  let index = 0;
  for (const type of types) {
    if (type === 'string' || type === 'bytes' || type in ARRAY_ELEMENT_BYTES) {
      const ptr = Number(args[index]);
      const length = Number(args[index + 1]);
      index += 2;
      if (type === 'string') decoded.push(memory.readUtf8(ptr, length));
      else if (type === 'bytes') decoded.push(memory.readBytes(ptr, length));
      else decoded.push(liftArray(type, memory.readBytes(ptr, length * (ARRAY_ELEMENT_BYTES[type] ?? 1))));
      continue;
    }
    decoded.push(type === 'bool' ? Number(args[index]) !== 0 : args[index]);
    index += 1;
  }
  return decoded;
}

function adaptExport(
  memory: AppMemory,
  name: string,
  target: ExportFunction,
  hostEventArgs: ReadonlyMap<string, readonly HostServiceTypeName[]>,
): ExportFunction {
  const eventTypes = hostEventArgs.get(name);
  if (eventTypes !== undefined) return (...args: unknown[]) => target(...decodeHostEventArguments(memory, eventTypes, args));
  const kinds = POINTER_EXPORTS[name];
  if (kinds === undefined) return target;
  return (...args: unknown[]) => target(...decodeArguments(memory, kinds, args));
}

function appFunction(app: NetWasmApp, name: string): ExportFunction | undefined {
  const value = app[name];
  return typeof value === 'function' ? (value as ExportFunction) : undefined;
}

function adaptExports(
  app: NetWasmApp,
  memory: AppMemory,
  hostEventArgs: ReadonlyMap<string, readonly HostServiceTypeName[]>,
): Record<string, unknown> {
  const exports: Record<string, unknown> = {
    memory: memory.memory,
    __fui_key_buffer: () => memory.keyBufferPtr,
    __fui_text_buffer: () => memory.textBufferPtr,
    __fui_text_buffer_size: () => memory.textBufferSize,
  };
  const missing: string[] = [];
  for (const [name, fallback] of Object.entries(REQUIRED_EXPORT_DEFAULTS)) {
    if (BUFFER_EXPORTS.has(name)) continue;
    const target = appFunction(app, name);
    if (target === undefined) {
      missing.push(name);
      exports[name] = () => fallback;
    } else {
      exports[name] = adaptExport(memory, name, target, hostEventArgs);
    }
  }
  for (const name of Object.keys(app)) {
    const target = appFunction(app, name);
    if (target === undefined || name in exports) continue;
    exports[name] = adaptExport(memory, name, target, hostEventArgs);
  }
  if (missing.length > 0) {
    console.debug(`fui-cs: ${String(missing.length)} harness exports are not implemented yet: ${missing.join(', ')}`);
  }
  return exports;
}

/**
 * The `__disposeApp` export: the harness disposes an application for good (a route swap or a runtime recreation opens
 * a new one), so after the application disposes its tree, its NetWasm module closes and the page drops its bridge, and
 * nothing of the application stays reachable.
 */
function closingDispose(app: NetWasmApp, bridge: FuiCsTier3Bridge, dispose: ExportFunction | undefined): ExportFunction {
  return () => {
    try {
      dispose?.();
    } finally {
      uninstallTier3Bridge(bridge);
      const close = app.close;
      if (typeof close === 'function') {
        (close as () => Promise<void>).call(app).catch((error: unknown) => {
          console.error('fui-cs: closing the application module failed', error);
        });
      }
    }
  };
}

export interface NetWasmAppOptions {
  /** The host events the page gives the harness, whose exports the application generated with hostgen. */
  readonly hostEvents?: HostEventsDefinition;
}

/** The managed harness `instantiateApp` hook for a fui-cs NetWasm JsModule. */
export function createNetWasmAppInstantiator(openApp: OpenNetWasmApp, options: NetWasmAppOptions = {}): WasmAppInstantiator {
  const hostEventArgs = new Map(listHostEventMethods(options.hostEvents).map((method) => [method.exportName, method.args] as const));
  return async (_module, imports) => {
    const memory = new AppMemory();
    const bridge: FuiCsTier3Bridge = { memory, imports };
    installTier3Bridge(bridge);
    const app = await openApp();
    installNetWasmWorkers(imports, memory, (workerId, kind, text) => {
      appFunction(app, '__fui_on_worker_message')?.(workerId, kind, text);
    });
    const exports = adaptExports(app, memory, hostEventArgs);
    exports.__disposeApp = closingDispose(app, bridge, appFunction(app, '__disposeApp'));
    const instance: unknown = { exports };
    return instance as WebAssembly.Instance;
  };
}
