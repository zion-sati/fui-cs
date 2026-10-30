// Scratch linear memory that stands in for the Tier 3 module's own memory.
//
// The managed harness exchanges strings and buffers with Tier 3 through pointers into the
// app's linear memory. A NetWasm JsModule does not expose its memory, so fui-cs gives the
// harness this one instead: the harness writes event text into the key and text buffers, and
// the fui-cs JSImport shims write call arguments into the argument arena.

const PAGE_BYTES = 65_536;
const KEY_BUFFER_PTR = 16;
const KEY_BUFFER_BYTES = 1_024;
const TEXT_BUFFER_PTR = KEY_BUFFER_PTR + KEY_BUFFER_BYTES;
const TEXT_BUFFER_BYTES = 64 * 1_024;
const ARENA_BASE = PAGE_BYTES * 2;
const INITIAL_PAGES = 4;

export type OutElement = 'bool' | 'f32' | 'f64' | 'i32' | 'u32' | 'u64';

export interface Allocation {
  readonly ptr: number;
  readonly len: number;
}

export class AppMemory {
  readonly memory = new WebAssembly.Memory({ initial: INITIAL_PAGES });
  readonly keyBufferPtr = KEY_BUFFER_PTR;
  readonly textBufferPtr = TEXT_BUFFER_PTR;
  readonly textBufferSize = TEXT_BUFFER_BYTES;
  private arenaTop = ARENA_BASE;
  private readonly encoder = new TextEncoder();
  private readonly decoder = new TextDecoder();

  /** Marks the arena so a call can release everything it allocated, nested calls included. */
  mark(): number {
    return this.arenaTop;
  }

  release(mark: number): void {
    this.arenaTop = mark;
  }

  allocate(bytes: number, alignment = 8): number {
    const ptr = Math.ceil(this.arenaTop / alignment) * alignment;
    const end = ptr + bytes;
    const shortfall = end - this.memory.buffer.byteLength;
    if (shortfall > 0) {
      this.memory.grow(Math.ceil(shortfall / PAGE_BYTES));
    }
    this.arenaTop = end;
    return ptr;
  }

  writeUtf8(text: string): Allocation {
    const bytes = this.encoder.encode(text);
    return this.writeBytes(bytes);
  }

  writeBytes(bytes: Uint8Array): Allocation {
    if (bytes.length === 0) return { ptr: 0, len: 0 };
    const ptr = this.allocate(bytes.length, 1);
    new Uint8Array(this.memory.buffer, ptr, bytes.length).set(bytes);
    return { ptr, len: bytes.length };
  }

  writeFloat32(values: Float32Array): Allocation {
    if (values.length === 0) return { ptr: 0, len: 0 };
    const ptr = this.allocate(values.byteLength, 4);
    new Float32Array(this.memory.buffer, ptr, values.length).set(values);
    return { ptr, len: values.length };
  }

  writeUint32(values: Uint32Array): Allocation {
    if (values.length === 0) return { ptr: 0, len: 0 };
    const ptr = this.allocate(values.byteLength, 4);
    new Uint32Array(this.memory.buffer, ptr, values.length).set(values);
    return { ptr, len: values.length };
  }

  readUtf8(ptr: number, len: number): string {
    if (len <= 0) return '';
    return this.decoder.decode(new Uint8Array(this.memory.buffer, ptr, len));
  }

  readBytes(ptr: number, len: number): Uint8Array {
    if (len <= 0) return new Uint8Array(0);
    return new Uint8Array(this.memory.buffer, ptr, len).slice();
  }

  readFloat32(ptr: number): number {
    return new DataView(this.memory.buffer).getFloat32(ptr, true);
  }

  readUint32(ptr: number): number {
    return new DataView(this.memory.buffer).getUint32(ptr, true);
  }

  /** Reads one ABI out value as a JavaScript number (64-bit values must stay below 2^53). */
  readScalar(ptr: number, element: OutElement): number {
    const view = new DataView(this.memory.buffer);
    switch (element) {
      case 'f32': return view.getFloat32(ptr, true);
      case 'f64': return view.getFloat64(ptr, true);
      case 'i32': return view.getInt32(ptr, true);
      case 'u32': return view.getUint32(ptr, true);
      case 'u64': return Number(view.getBigUint64(ptr, true));
      case 'bool': return view.getUint8(ptr);
    }
  }
}
