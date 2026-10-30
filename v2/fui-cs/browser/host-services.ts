// Application host services: the generated JSImport modules call these to reach the services a page defines
// with defineHostServices, through the managed harness's `fui_host_service` imports.
//
// NetWasm hands each JSImport plain JavaScript values (strings, numbers, bigints, booleans and typed arrays). The
// harness's imports take strings and arrays as (pointer, length) pairs in the app memory and write string and array
// results into a caller buffer, so this lowers the call into the argument arena and lifts the result back out.

import type { AppMemory } from './app-memory.ts';
import { harnessImport, tier3Bridge } from './tier3-contract.ts';

export type HostServiceType =
  | 'string'
  | 'bool'
  | 'i32'
  | 'u32'
  | 'i64'
  | 'u64'
  | 'f64'
  | 'bytes'
  | 'i32_array'
  | 'u32_array'
  | 'i64_array'
  | 'u64_array'
  | 'f64_array'
  | 'void';

type TypedArray = Uint8Array | Int32Array | Uint32Array | BigInt64Array | BigUint64Array | Float64Array;

const HOST_SERVICE_MODULE = 'fui_host_service';

function isBufferType(type: HostServiceType): boolean {
  return type === 'string' || type === 'bytes' || type.endsWith('_array');
}

function bytesOf(value: TypedArray): Uint8Array {
  return new Uint8Array(value.buffer, value.byteOffset, value.byteLength);
}

function lowerArgument(memory: AppMemory, type: HostServiceType, value: unknown, raw: unknown[]): void {
  if (type === 'string') {
    const text = memory.writeUtf8(value as string);
    raw.push(text.ptr, text.len);
  } else if (type === 'bytes' || type.endsWith('_array')) {
    // Byte arrays pass their length in bytes; number arrays pass their element count.
    const array = value as TypedArray;
    const payload = memory.writeBytes(bytesOf(array));
    raw.push(payload.ptr, array.length);
  } else if (type === 'bool') {
    raw.push(value === true ? 1 : 0);
  } else {
    raw.push(value);
  }
}

/** Lifts a string or array result the harness wrote into the result buffer; its length is in bytes. */
function liftBuffer(memory: AppMemory, type: HostServiceType, ptr: number, length: number): unknown {
  if (type === 'string') return memory.readUtf8(ptr, length);
  const bytes = memory.readBytes(ptr, length);
  switch (type) {
    case 'i32_array': return new Int32Array(bytes.buffer);
    case 'u32_array': return new Uint32Array(bytes.buffer);
    case 'i64_array': return new BigInt64Array(bytes.buffer);
    case 'u64_array': return new BigUint64Array(bytes.buffer);
    case 'f64_array': return new Float64Array(bytes.buffer);
    default: return bytes;
  }
}

/**
 * Calls the host service `importName` with `args` of the given types and returns its result as the JavaScript value
 * NetWasm marshals into the C# return type. String and array results are limited to the shared text buffer's size.
 */
export function callHostService(
  importName: string,
  argTypes: readonly HostServiceType[],
  args: readonly unknown[],
  returns: HostServiceType,
): unknown {
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    const raw: unknown[] = [];
    argTypes.forEach((type, index) => { lowerArgument(memory, type, args[index], raw); });
    const resultPtr = isBufferType(returns) ? memory.allocate(memory.textBufferSize) : 0;
    if (resultPtr !== 0) raw.push(resultPtr, memory.textBufferSize);
    const result = harnessImport(HOST_SERVICE_MODULE, importName)(...raw);
    if (returns === 'void') return undefined;
    if (returns === 'bool') return Number(result) !== 0;
    if (resultPtr !== 0) return liftBuffer(memory, returns, resultPtr, Number(result));
    return result;
  } finally {
    memory.release(mark);
  }
}
