// The host page's probe services and events for the `host` browser test page: one of each host-service type,
// as arguments, results and event values. Tests raise the events and read the recorded calls through
// window.__fuiProbe.

import {
  defineHostEvents,
  defineHostServices,
  hostEvent,
  hostService,
  type HostServiceMethodDefinition,
  type HostServiceTypeName,
} from '@effindomv2/runtime/managed-harness';

export interface ProbeControls {
  /** Raises probe.everything with one value of each type. */
  emitEverything(): void;
  /** Raises probe.pinged, which carries no values. */
  ping(): void;
  /** What probe.record received, one "label:values" entry per call. */
  readonly calls: string[];
}

declare global {
  interface Window {
    __fuiProbe?: ProbeControls;
  }
}

type EverythingEmitter = (
  text: string,
  flag: boolean,
  int: number,
  uint: number,
  long: bigint,
  ulong: bigint,
  real: number,
  bytes: Uint8Array,
  ints: Int32Array,
  uints: Uint32Array,
  longs: BigInt64Array,
  ulongs: BigUint64Array,
  reals: Float64Array,
) => void;

/**
 * defineHostServices cannot take a typed method with arguments: the general definition's implementation accepts
 * any host value, so a typed implementation's parameters do not fit it. Each method is typed where it is written,
 * then widened here.
 */
function widen<TArgs extends readonly HostServiceTypeName[], TResult extends HostServiceTypeName>(
  method: HostServiceMethodDefinition<TArgs, TResult>,
): HostServiceMethodDefinition {
  return method as unknown as HostServiceMethodDefinition;
}

const everythingEmitters = new Set<EverythingEmitter>();
const pingEmitters = new Set<() => void>();
const calls: string[] = [];

export const probeHostServices = defineHostServices({
  probe: {
    greet: widen(hostService({ args: ['string'] as const, returns: 'string', implementation: (name) => `Hello, ${name}` })),
    invert: widen(hostService({ args: ['bool'] as const, returns: 'bool', implementation: (value) => !value })),
    add: widen(hostService({ args: ['i32', 'i32'] as const, returns: 'i32', implementation: (left, right) => (left + right) | 0 })),
    complement: widen(hostService({ args: ['u32'] as const, returns: 'u32', implementation: (value) => ~value >>> 0 })),
    negateLong: widen(hostService({ args: ['i64'] as const, returns: 'i64', implementation: (value) => -value })),
    nextUlong: widen(hostService({ args: ['u64'] as const, returns: 'u64', implementation: (value) => value + 1n })),
    half: widen(hostService({ args: ['f64'] as const, returns: 'f64', implementation: (value) => value / 2 })),
    reverseBytes: widen(hostService({ args: ['bytes'] as const, returns: 'bytes', implementation: (bytes) => bytes.slice().reverse() })),
    doubleInts: widen(hostService({ args: ['i32_array'] as const, returns: 'i32_array', implementation: (values) => values.map((value) => value * 2) })),
    complementUints: widen(hostService({ args: ['u32_array'] as const, returns: 'u32_array', implementation: (values) => values.map((value) => ~value >>> 0) })),
    negateLongs: widen(hostService({ args: ['i64_array'] as const, returns: 'i64_array', implementation: (values) => values.map((value) => -value) })),
    incrementUlongs: widen(hostService({ args: ['u64_array'] as const, returns: 'u64_array', implementation: (values) => values.map((value) => value + 1n) })),
    halveDoubles: widen(hostService({ args: ['f64_array'] as const, returns: 'f64_array', implementation: (values) => values.map((value) => value / 2) })),
    record: widen(hostService({
      args: ['string', 'u32_array'] as const,
      returns: 'void',
      implementation: (label, values) => {
        calls.push(`${label}:${values.join(',')}`);
        return undefined;
      },
    })),
  },
});

export const probeHostEvents = defineHostEvents({
  probe: {
    everything: hostEvent({
      args: ['string', 'bool', 'i32', 'u32', 'i64', 'u64', 'f64', 'bytes', 'i32_array', 'u32_array', 'i64_array', 'u64_array', 'f64_array'] as const,
      subscribe(emit) {
        everythingEmitters.add(emit);
        return () => {
          everythingEmitters.delete(emit);
        };
      },
    }),
    pinged: hostEvent({
      args: [] as const,
      subscribe(emit) {
        pingEmitters.add(emit);
        return () => {
          pingEmitters.delete(emit);
        };
      },
    }),
  },
});

export function installProbeControls(): void {
  window.__fuiProbe = {
    emitEverything(): void {
      for (const emit of everythingEmitters) {
        emit('héllo ✓', true, -7, 4_294_967_295, -9_007_199_254_740_993n, 18_446_744_073_709_551_615n, 0.25,
          new Uint8Array([9, 8]), new Int32Array([-1, 2]), new Uint32Array([4_294_967_295]),
          new BigInt64Array([-5n]), new BigUint64Array([18_446_744_073_709_551_615n]), new Float64Array([0.5]));
      }
    },
    ping(): void {
      for (const emit of pingEmitters) emit();
    },
    calls,
  };
}
