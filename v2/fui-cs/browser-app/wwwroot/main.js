import { dotnet } from './_framework/dotnet.js';

const waitForFrame = () => new Promise((resolve) => {
  requestAnimationFrame(() => {
    requestAnimationFrame(resolve);
  });
});

function toBigIntHandle(handle) {
  if (typeof handle === 'bigint') return handle;
  if (typeof handle === 'number') return BigInt(handle);
  if (typeof handle === 'string') return BigInt(handle);
  const primitive = handle.valueOf();
  if (typeof primitive === 'bigint') return primitive;
  if (typeof primitive === 'number') return BigInt(primitive);
  if (typeof primitive === 'string') return BigInt(primitive);
  return BigInt(handle.toString());
}

function pointerToHeapOffset(pointer) {
  if (typeof pointer === 'number') return pointer;
  return Number(toBigIntHandle(pointer));
}

function normalizePointerForWasm(module, pointer) {
  return module.usesMemory64 === true ? toBigIntHandle(pointer) : pointerToHeapOffset(pointer);
}

function findExport(exportsObject, name) {
  if (exportsObject === null || typeof exportsObject !== 'object') {
    return null;
  }
  if (typeof exportsObject[name] === 'function') {
    return exportsObject[name];
  }
  for (const value of Object.values(exportsObject)) {
    const match = findExport(value, name);
    if (match !== null) {
      return match;
    }
  }
  return null;
}

function updateState(commandWords, rootHandle) {
  window.__fuiCsState = {
    commandWordCount: commandWords.length,
    commandWords,
    rootHandle,
  };
}

function formatBootError(error) {
  if (error instanceof Error) return error.message;
  if (typeof error === 'string') return error;
  try {
    return JSON.stringify(error);
  } catch {
    return String(error);
  }
}

function createUiImports(runtime) {
  let latestRootHandle = null;
  return {
    ui_reset() {
      runtime.ui._ui_reset();
      updateState([], null);
    },
    ui_create_node(type) {
      return toBigIntHandle(runtime.ui._ui_create_node(type));
    },
    ui_delete_node(handle) {
      runtime.ui._ui_delete_node(toBigIntHandle(handle));
    },
    ui_node_add_child(parent, child) {
      runtime.ui._ui_node_add_child(toBigIntHandle(parent), toBigIntHandle(child));
    },
    ui_node_remove_child(parent, child) {
      runtime.ui._ui_node_remove_child(toBigIntHandle(parent), toBigIntHandle(child));
    },
    ui_set_root(handle) {
      const rootHandle = toBigIntHandle(handle);
      latestRootHandle = rootHandle.toString();
      runtime.ui._ui_set_root(rootHandle);
      updateState([], latestRootHandle);
    },
    ui_set_width(handle, value, unit) {
      runtime.ui._ui_set_width(toBigIntHandle(handle), value, unit);
    },
    ui_set_height(handle, value, unit) {
      runtime.ui._ui_set_height(toBigIntHandle(handle), value, unit);
    },
    ui_set_bg_color(handle, color) {
      runtime.ui._ui_set_bg_color(toBigIntHandle(handle), color >>> 0);
    },
    ui_set_text(handle, text) {
      if (text.length === 0) {
        runtime.ui._ui_set_text(toBigIntHandle(handle), 0, 0);
        return;
      }
      const bytes = new TextEncoder().encode(text);
      const rawPtr = runtime.ui._malloc(bytes.length);
      const ptr = normalizePointerForWasm(runtime.ui, rawPtr);
      const offset = pointerToHeapOffset(ptr);
      runtime.ui.refreshHeapViews?.();
      runtime.ui.HEAPU8.set(bytes, offset);
      runtime.ui._ui_set_text(toBigIntHandle(handle), ptr, bytes.length);
      runtime.ui._free(rawPtr);
    },
    ui_set_font(handle, fontId, size) {
      runtime.ui._ui_set_font(toBigIntHandle(handle), fontId, size);
    },
    ui_set_text_color(handle, color) {
      runtime.ui._ui_set_text_color(toBigIntHandle(handle), color >>> 0);
    },
    ui_set_padding(handle, top, right, bottom, left) {
      runtime.ui._ui_set_padding(toBigIntHandle(handle), top, right, bottom, left);
    },
    ui_set_flex_direction(handle, direction) {
      runtime.ui._ui_set_flex_direction(toBigIntHandle(handle), direction);
    },
    ui_commit_frame() {
      runtime.commitFrame();
      const commandWords = [1];
      updateState(commandWords, latestRootHandle);
    },
    ui_resize_window(width, height) {
      runtime.ui._ui_resize_window(width, height);
    },
  };
}

function createHostImports(runtime) {
  return {
    request_render() {
      runtime.requestFrame();
    },
    get_viewport_width() {
      const rect = runtime.canvas.getBoundingClientRect();
      return rect.width > 0 ? rect.width : runtime.canvas.width;
    },
    get_viewport_height() {
      const rect = runtime.canvas.getBoundingClientRect();
      return rect.height > 0 ? rect.height : runtime.canvas.height;
    },
  };
}

async function boot() {
  const runtime = await window.EffinDomBrowserBridge.ready;
  const { setModuleImports, getAssemblyExports, getConfig, runMain } = await dotnet.create();

  setModuleImports('main.js', {
    effindom_v2_ui: createUiImports(runtime),
    fui_host: createHostImports(runtime),
  });

  const config = getConfig();
  const exports = await getAssemblyExports(config.mainAssemblyName);
  await runMain();

  const runSmokeApp = findExport(exports, 'RunSmokeApp');
  if (runSmokeApp === null) {
    throw new Error('FUI-CS RunSmokeApp export was not found.');
  }

  runtime.resetLogs();
  runSmokeApp();
  await waitForFrame();

  window.__fuiCsReady = true;
  delete window.__fuiCsError;
}

boot().catch((error) => {
  const message = formatBootError(error);
  window.__fuiCsError = message;
  throw error;
});
