// The test worker's host services, which run inside the worker: a string, a 64-bit integer, number arrays, a
// boolean and bytes in and out, so the worker test page proves each JavaScript value crosses the worker boundary.

import {
  defineHostServices,
  hostService,
  type HostServiceMethodDefinition,
  type HostServiceTypeName,
} from '@effindomv2/runtime/managed-harness';

// defineHostServices cannot take a typed method with arguments (see browser/probe-host.ts); each method is typed
// where it is written, then widened here.
function widen<TArgs extends readonly HostServiceTypeName[], TResult extends HostServiceTypeName>(
  method: HostServiceMethodDefinition<TArgs, TResult>,
): HostServiceMethodDefinition {
  return method as unknown as HostServiceMethodDefinition;
}

export const probeWorkerHostServices = defineHostServices({
  probeWorker: {
    describe: widen(hostService({
      args: ['string', 'i64', 'u32_array', 'bool'] as const,
      returns: 'string',
      implementation: (text, long, values, flag) => `${text}:${long.toString()}:${Array.from(values).join(',')}:${String(flag)}`,
    })),
    scale: widen(hostService({
      args: ['f64_array', 'f64'] as const,
      returns: 'f64_array',
      implementation: (values, factor) => values.map((value) => value * factor),
    })),
    reverseBytes: widen(hostService({
      args: ['bytes'] as const,
      returns: 'bytes',
      implementation: (bytes) => bytes.slice().reverse(),
    })),
    inWorker: widen(hostService({
      args: [] as const,
      returns: 'bool',
      implementation: () => typeof (globalThis as { document?: unknown }).document === 'undefined',
    })),
  },
});
