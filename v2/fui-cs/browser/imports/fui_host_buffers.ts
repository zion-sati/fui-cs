// JSImport module `fui_host_buffers`: host functions that fill caller-sized buffers, which the ABI generator
// cannot project. Each retries with the size the host reports when the first buffer is too small.

import { harnessImport, tier3Bridge } from '../tier3-contract.ts';

const MODULE = 'fui_host';
const UI_MODULE = 'effindom_v2_ui';
const RECT_FLOATS = 4;
const INITIAL_PAYLOAD_BYTES = 256;

interface PersistedTextState {
  readonly version: number;
  readonly payload: string;
}

function copyPersistedState(nodeId: string, kind: string): PersistedTextState | null {
  const { memory } = tier3Bridge();
  let capacity = INITIAL_PAYLOAD_BYTES;
  for (;;) {
    const mark = memory.mark();
    try {
      const id = memory.writeUtf8(nodeId);
      const kindBytes = memory.writeUtf8(kind);
      const versionPtr = memory.allocate(4, 4);
      const payloadPtr = memory.allocate(capacity, 1);
      const copied = Number(harnessImport(MODULE, 'fui_copy_persisted_state')(
        id.ptr, id.len, kindBytes.ptr, kindBytes.len, versionPtr, payloadPtr, capacity));
      if (copied < 0) return null;
      if (copied > capacity) {
        capacity = copied;
        continue;
      }
      return { version: memory.readScalar(versionPtr, 'u32'), payload: memory.readUtf8(payloadPtr, copied) };
    } finally {
      memory.release(mark);
    }
  }
}

/** The rectangles a text range covers, four floats per line, sized by the host's own count. */
export function fui_copy_text_range_rects(handle: bigint, start: number, end: number): Float64Array {
  const count = Number(harnessImport(UI_MODULE, 'ui_get_text_range_rect_count')(handle, start, end));
  if (count <= 0) return new Float64Array(0);
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    const rectsPtr = memory.allocate(count * RECT_FLOATS * 4, 4);
    const copied = Number(harnessImport(UI_MODULE, 'ui_copy_text_range_rects')(handle, start, end, rectsPtr, count));
    const rects = new Float64Array(Math.min(copied, count) * RECT_FLOATS);
    for (let index = 0; index < rects.length; index += 1) {
      rects[index] = memory.readFloat32(rectsPtr + (index * 4));
    }
    return rects;
  } finally {
    memory.release(mark);
  }
}

/** A selection area's start and end endpoint rectangles, eight floats; empty when it has no cross selection. */
export function fui_copy_cross_selection_endpoint_rects(areaHandle: bigint): Float64Array {
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    const rectsPtr = memory.allocate(2 * RECT_FLOATS * 4, 4);
    if (!harnessImport(UI_MODULE, 'ui_copy_cross_selection_endpoint_rects')(areaHandle, rectsPtr)) return new Float64Array(0);
    const rects = new Float64Array(2 * RECT_FLOATS);
    for (let index = 0; index < rects.length; index += 1) {
      rects[index] = memory.readFloat32(rectsPtr + (index * 4));
    }
    return rects;
  } finally {
    memory.release(mark);
  }
}

/** The canvas token that draws onto an offscreen canvas. */
export function fui_get_offscreen_canvas_token(offscreenId: number): number {
  return Number(harnessImport(MODULE, 'fui_canvas_get_offscreen_ptr')(offscreenId));
}

/** An offscreen canvas's RGBA pixels. */
export function fui_read_offscreen_pixels(offscreenId: number, width: number, height: number): Uint8Array {
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    const length = width * height * 4;
    const pixelsPtr = memory.allocate(length, 4);
    harnessImport(MODULE, 'fui_canvas_read_offscreen_pixels')(offscreenId, pixelsPtr, width, height);
    return memory.readBytes(pixelsPtr, length);
  } finally {
    memory.release(mark);
  }
}

/** A built node rendered into RGBA pixels; empty when the host cannot render it. */
export function fui_render_node_to_rgba(handle: bigint, width: number, height: number, scale: number, x: number, y: number): Uint8Array {
  const { memory } = tier3Bridge();
  const mark = memory.mark();
  try {
    const length = width * height * 4;
    const pixelsPtr = memory.allocate(length, 4);
    const written = Number(harnessImport(MODULE, 'fui_render_node_to_rgba')(handle, width, height, pixelsPtr, length, scale, x, y));
    return written === 0 ? new Uint8Array(0) : memory.readBytes(pixelsPtr, length);
  } finally {
    memory.release(mark);
  }
}

export function fui_copy_persisted_state_payload(nodeId: string, kind: string): string | null {
  return copyPersistedState(nodeId, kind)?.payload ?? null;
}

export function fui_copy_persisted_state_version(nodeId: string, kind: string): number {
  return copyPersistedState(nodeId, kind)?.version ?? 0;
}
