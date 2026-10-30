// A worker's host services: the generated module of a worker's definitions hands NetWasm their implementations
// under their import names. NetWasm passes a worker's JSImport the JavaScript values the definitions take
// (strings, numbers, bigints for 64-bit integers, booleans and typed arrays) and accepts the same values back.

import { listHostServiceMethods, type HostServicesDefinition } from '@effindomv2/runtime/managed-harness';

export function workerHostServiceImports(services: HostServicesDefinition): Record<string, (...args: never[]) => unknown> {
  const imports: Record<string, (...args: never[]) => unknown> = {};
  for (const method of listHostServiceMethods(services)) {
    imports[method.importName] = (...args: never[]) => method.implementation(...(args as readonly unknown[]));
  }
  return imports;
}
