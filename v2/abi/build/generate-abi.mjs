// v2/abi/shared/core.ts
import { promises as fs } from "node:fs";
import path from "node:path";
function toPosix(value) {
  return value.split(path.sep).join("/");
}
async function pathExists(candidate) {
  try {
    await fs.access(candidate);
    return true;
  } catch {
    return false;
  }
}
async function findRepoRoot(startPath) {
  let current = path.resolve(startPath);
  while (current !== path.dirname(current)) {
    if (await pathExists(path.join(current, "package.json")) && await pathExists(path.join(current, "v2/ui/include/effindom_ui.h"))) {
      return current;
    }
    current = path.dirname(current);
  }
  throw new Error(`Could not locate repo root from ${startPath}.`);
}
function getArg(args, name) {
  const prefix = `--${name}=`;
  return args.find((arg) => arg.startsWith(prefix))?.slice(prefix.length);
}
function sourcePathForHeader(packageDir, headerPath) {
  const relative = path.relative(packageDir, headerPath);
  return toPosix(relative.startsWith(".") ? relative : `./${relative}`);
}
async function writeGeneratedFile(outputPath, generated, check, staleMessage) {
  if (check) {
    const current = await fs.readFile(outputPath, "utf8").catch(() => "");
    if (current !== generated) {
      throw new Error(staleMessage);
    }
    return;
  }
  await fs.mkdir(path.dirname(outputPath), { recursive: true });
  await fs.writeFile(outputPath, generated);
}
function parseCommand(args, strategies2) {
  const strategyName = args.find((arg) => !arg.startsWith("--")) ?? "";
  if (strategyName.length === 0) {
    throw new Error(`Missing generator strategy. Available strategies: ${strategies2.map((strategy) => strategy.name).join(", ")}`);
  }
  return {
    strategyName,
    check: args.includes("--check"),
    rest: args.filter((arg) => arg !== strategyName && arg !== "--check")
  };
}

// v2/abi/fui-as/strategies/ui.ts
import { promises as fs4 } from "node:fs";
import path4 from "node:path";

// v2/abi/shared/assemblyscript.ts
import { promises as fs2 } from "node:fs";
import path2 from "node:path";

// v2/abi/shared/c-header.ts
function normalizeCExpression(value) {
  return value.trim().replace(/\b([0-9]+)U\b/g, "$1");
}
function normalizeCType(value) {
  return value.replace(/\s+/g, " ").replace(/\s*\*\s*/g, " * ").trim();
}
function stripComments(source) {
  return source.replace(/\/\*[\s\S]*?\*\//g, "").replace(/\/\/[^\n\r]*/g, "");
}
function parseEnumBody(body) {
  const members = [];
  let nextImplicitValue = 0;
  for (const rawPart of body.split(",")) {
    const part = rawPart.trim();
    if (part.length === 0) {
      continue;
    }
    const match = /^([A-Za-z_]\w*)(?:\s*=\s*(.+))?$/.exec(part);
    if (match === null) {
      throw new Error(`Could not parse enum member: ${part}`);
    }
    const name = match[1];
    const explicitValue = part.includes("=") ? match[2] : "";
    const value = explicitValue.length === 0 ? String(nextImplicitValue) : normalizeCExpression(explicitValue);
    members.push({ name, value });
    const numeric = Number(value);
    nextImplicitValue = Number.isFinite(numeric) ? numeric + 1 : nextImplicitValue + 1;
  }
  return members;
}
function parseConstants(source) {
  const constants = [];
  const anonymousEnumPattern = /enum\s*\{([\s\S]*?)\}\s*;/g;
  let match;
  while ((match = anonymousEnumPattern.exec(source)) !== null) {
    constants.push(...parseEnumBody(match[1]));
  }
  return constants;
}
function parseEnums(source) {
  const enums = [];
  const enumPattern = /typedef\s+enum\s+([A-Za-z_]\w*)\s*\{([\s\S]*?)\}\s*([A-Za-z_]\w*)\s*;/g;
  let match;
  while ((match = enumPattern.exec(source)) !== null) {
    const enumName = match[3].length > 0 ? match[3] : match[1];
    enums.push({
      name: enumName,
      members: parseEnumBody(match[2])
    });
  }
  return enums;
}
function splitParams(paramsSource) {
  const trimmed = paramsSource.trim();
  if (trimmed.length === 0 || trimmed === "void") {
    return [];
  }
  return trimmed.split(",").map((part) => part.trim()).filter((part) => part.length > 0);
}
function parseParam(paramSource) {
  const withoutDefault = paramSource.replace(/\s*=\s*[^,]+$/, "").trim();
  const normalized = normalizeCType(withoutDefault);
  const match = /^(.+)\s+([A-Za-z_]\w*)$/.exec(normalized);
  if (match === null) {
    throw new Error(`Could not parse function parameter: ${paramSource}`);
  }
  return {
    type: normalizeCType(match[1]),
    name: match[2]
  };
}
function parseFunctions(source) {
  const functions = [];
  const seen = /* @__PURE__ */ new Set();
  const withoutPreprocessorLines = source.split(/\r?\n/).filter((line) => !line.trim().startsWith("#")).join("\n");
  const functionPattern = /([A-Za-z_]\w*(?:\s+\*?|\s*\*\s*)+)\s*([A-Za-z_]\w*)\s*\(([^;{}()]*)\)\s*;/g;
  let match;
  while ((match = functionPattern.exec(withoutPreprocessorLines)) !== null) {
    const name = match[2];
    if (seen.has(name)) {
      continue;
    }
    seen.add(name);
    functions.push({
      name,
      returnType: normalizeCType(match[1]),
      params: splitParams(match[3]).map(parseParam)
    });
  }
  return functions;
}
function parseCHeader(source) {
  const cleanSource = stripComments(source);
  return {
    constants: parseConstants(cleanSource),
    enums: parseEnums(cleanSource),
    functions: parseFunctions(cleanSource)
  };
}

// v2/abi/shared/assemblyscript.ts
function asType(cType) {
  const normalized = normalizeCType(cType).replace(/^const\s+/, "");
  if (normalized.includes("*")) {
    return "usize";
  }
  switch (normalized) {
    case "void":
      return "void";
    case "bool":
      return "bool";
    case "float":
      return "f32";
    case "double":
      return "f64";
    case "int32_t":
      return "i32";
    case "uint32_t":
    case "ui_color_t":
      return "u32";
    case "uint64_t":
    case "ui_handle_t":
      return "u64";
    case "uintptr_t":
      return "usize";
    default:
      if (normalized.startsWith("Ui") || normalized.startsWith("Ed")) {
        return "u32";
      }
      throw new Error(`Unsupported C ABI type: ${cType}`);
  }
}
function asScalarType(type) {
  return type;
}
function emitAssemblyScriptExternal(moduleName, fn) {
  const params = fn.name === "ui_commit_frame" ? [] : fn.params.map((param) => `${param.name}: ${asType(param.type)}`);
  const returnType = asType(fn.returnType);
  return [
    `@external("${moduleName}", "${fn.name}")`,
    `export declare function ${fn.name}(${params.join(", ")}): ${returnType};`
  ].join("\n");
}
function emitAssemblyScriptHostExternal(moduleName, item) {
  const params = item.args.map((param) => `${param.name}: ${asScalarType(param.type)}`);
  return [
    `@external("${moduleName}", "${item.importName}")`,
    `export declare function ${item.name}(${params.join(", ")}): ${asScalarType(item.returns)};`
  ].join("\n");
}
async function collectUsedAssemblyScriptFfiImports(sourcePath, outputPath, prefix) {
  const used = /* @__PURE__ */ new Set();
  async function visit(currentPath) {
    const entries = await fs2.readdir(currentPath, { withFileTypes: true });
    await Promise.all(entries.map(async (entry) => {
      const entryPath = path2.join(currentPath, entry.name);
      if (entry.isDirectory()) {
        if (entry.name === "generated" || entry.name === "build" || entry.name === "node_modules") {
          return;
        }
        await visit(entryPath);
        return;
      }
      if (!entry.isFile() || !entry.name.endsWith(".ts") || entryPath === outputPath) {
        return;
      }
      const text = await fs2.readFile(entryPath, "utf8");
      const pattern = new RegExp(`\\bffi\\.(${prefix}[A-Za-z_]\\w*)\\b`, "g");
      for (const match of text.matchAll(pattern)) {
        used.add(match[1]);
      }
    }));
  }
  await visit(sourcePath);
  return used;
}
function selectUsedFunctions(functions, usedFunctions) {
  const byName = new Map(functions.filter((fn) => fn.name.startsWith("ui_")).map((fn) => [fn.name, fn]));
  const requested = [...usedFunctions];
  const missing = requested.filter((name) => !byName.has(name)).sort();
  if (missing.length > 0) {
    throw new Error(`FUI-AS references UI ABI imports missing from effindom_ui.h: ${missing.join(", ")}`);
  }
  return functions.filter((fn) => requested.includes(fn.name));
}

// v2/abi/fui-as/strategies/common.ts
import { promises as fs3 } from "node:fs";
import path3 from "node:path";

// v2/abi/shared/fui-host.ts
var fuiHostEnums = [
  {
    name: "FuiCursorStyle",
    members: [
      { name: "FUI_CURSOR_DEFAULT", value: "0" },
      { name: "FUI_CURSOR_POINTER", value: "1" },
      { name: "FUI_CURSOR_TEXT", value: "2" },
      { name: "FUI_CURSOR_MOVE", value: "3" },
      { name: "FUI_CURSOR_GRAB", value: "4" },
      { name: "FUI_CURSOR_GRABBING", value: "5" },
      { name: "FUI_CURSOR_RESIZE_NS", value: "6" },
      { name: "FUI_CURSOR_RESIZE_EW", value: "7" }
    ]
  }
];
var fuiHostImports = [
  {
    name: "request_render",
    importName: "request_render",
    args: [],
    returns: "void"
  },
  {
    name: "get_viewport_width",
    importName: "get_viewport_width",
    args: [],
    returns: "f32"
  },
  {
    name: "get_viewport_height",
    importName: "get_viewport_height",
    args: [],
    returns: "f32"
  },
  {
    name: "get_device_pixel_ratio",
    importName: "get_device_pixel_ratio",
    args: [],
    returns: "f32"
  },
  {
    name: "fui_set_pointer_capture",
    importName: "fui_set_pointer_capture",
    args: [{ name: "handle", type: "u64" }],
    returns: "void"
  },
  {
    name: "fui_release_pointer_capture",
    importName: "fui_release_pointer_capture",
    args: [],
    returns: "void"
  },
  {
    name: "fui_reload_page",
    importName: "fui_reload_page",
    args: [],
    returns: "void"
  },
  {
    name: "fui_can_navigate_back",
    importName: "fui_can_navigate_back",
    args: [],
    returns: "bool"
  },
  {
    name: "fui_can_navigate_forward",
    importName: "fui_can_navigate_forward",
    args: [],
    returns: "bool"
  },
  {
    name: "fui_navigate_back",
    importName: "fui_navigate_back",
    args: [],
    returns: "void"
  },
  {
    name: "fui_navigate_forward",
    importName: "fui_navigate_forward",
    args: [],
    returns: "void"
  },
  {
    name: "fui_copy_text",
    importName: "fui_copy_text",
    args: [{ name: "ptr", type: "usize" }, { name: "len", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_has_text_selection_snapshot",
    importName: "fui_has_text_selection_snapshot",
    args: [{ name: "handle", type: "u64" }],
    returns: "bool"
  },
  {
    name: "fui_freeze_text_selection_snapshot",
    importName: "fui_freeze_text_selection_snapshot",
    args: [{ name: "handle", type: "u64" }],
    returns: "void"
  },
  {
    name: "fui_copy_text_selection_snapshot",
    importName: "fui_copy_text_selection_snapshot",
    args: [{ name: "handle", type: "u64" }],
    returns: "bool"
  },
  {
    name: "fui_cut_focused_text_selection",
    importName: "fui_cut_focused_text_selection",
    args: [],
    returns: "bool"
  },
  {
    name: "fui_cut_text_selection_snapshot",
    importName: "fui_cut_text_selection_snapshot",
    args: [{ name: "handle", type: "u64" }],
    returns: "bool"
  },
  {
    name: "fui_cut_text_range_snapshot",
    importName: "fui_cut_text_range_snapshot",
    args: [{ name: "handle", type: "u64" }, { name: "start", type: "u32" }, { name: "end", type: "u32" }],
    returns: "bool"
  },
  {
    name: "fui_delete_focused_text_range",
    importName: "fui_delete_focused_text_range",
    args: [{ name: "start", type: "u32" }, { name: "end", type: "u32" }],
    returns: "bool"
  },
  {
    name: "fui_commit_text_action_focus",
    importName: "fui_commit_text_action_focus",
    args: [{ name: "handle", type: "u64" }],
    returns: "void"
  },
  {
    name: "fui_load_svg",
    importName: "fui_load_svg",
    args: [{ name: "svgId", type: "u32" }, { name: "ptr", type: "usize" }, { name: "len", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_load_texture",
    importName: "fui_load_texture",
    args: [{ name: "textureId", type: "u32" }, { name: "ptr", type: "usize" }, { name: "len", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_release_svg",
    importName: "fui_release_svg",
    args: [{ name: "svgId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_release_texture",
    importName: "fui_release_texture",
    args: [{ name: "textureId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_bitmap_commit",
    importName: "fui_bitmap_commit",
    args: [{ name: "textureId", type: "u32" }, { name: "bytesPtr", type: "usize" }, { name: "bytesLen", type: "u32" }, { name: "width", type: "u32" }, { name: "height", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_bitmap_commit_dirty",
    importName: "fui_bitmap_commit_dirty",
    args: [{ name: "textureId", type: "u32" }, { name: "bytesPtr", type: "usize" }, { name: "bytesLen", type: "u32" }, { name: "fullW", type: "u32" }, { name: "fullH", type: "u32" }, { name: "subX", type: "u32" }, { name: "subY", type: "u32" }, { name: "subW", type: "u32" }, { name: "subH", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_bitmap_release",
    importName: "fui_bitmap_release",
    args: [{ name: "textureId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_render_node_to_rgba",
    importName: "fui_render_node_to_rgba",
    args: [{ name: "handle", type: "u64" }, { name: "width", type: "u32" }, { name: "height", type: "u32" }, { name: "outPtr", type: "usize" }, { name: "outCapacity", type: "u32" }, { name: "scale", type: "f32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "u32"
  },
  {
    name: "fui_load_font",
    importName: "fui_load_font",
    args: [{ name: "fontId", type: "u32" }, { name: "ptr", type: "usize" }, { name: "len", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_start_timer",
    importName: "fui_start_timer",
    args: [{ name: "timerId", type: "u32" }, { name: "delayMs", type: "i32" }],
    returns: "void"
  },
  {
    name: "fui_cancel_timer",
    importName: "fui_cancel_timer",
    args: [{ name: "timerId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_set_cursor",
    importName: "fui_set_cursor",
    args: [{ name: "style", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_show_url_preview",
    importName: "fui_show_url_preview",
    args: [{ name: "ptr", type: "usize" }, { name: "len", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_hide_url_preview",
    importName: "fui_hide_url_preview",
    args: [],
    returns: "void"
  },
  {
    name: "fui_navigate_to",
    importName: "fui_navigate_to",
    args: [{ name: "ptr", type: "usize" }, { name: "len", type: "u32" }, { name: "openInNewTab", type: "bool" }],
    returns: "void"
  },
  {
    name: "fui_set_persisted_scroll_offset",
    importName: "fui_set_persisted_scroll_offset",
    args: [{ name: "nodeIdPtr", type: "usize" }, { name: "nodeIdLen", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_try_get_persisted_scroll_offset",
    importName: "fui_try_get_persisted_scroll_offset",
    args: [{ name: "nodeIdPtr", type: "usize" }, { name: "nodeIdLen", type: "u32" }, { name: "outX", type: "usize" }, { name: "outY", type: "usize" }],
    returns: "bool"
  },
  {
    name: "fui_set_persisted_state",
    importName: "fui_set_persisted_state",
    args: [{ name: "nodeIdPtr", type: "usize" }, { name: "nodeIdLen", type: "u32" }, { name: "kindPtr", type: "usize" }, { name: "kindLen", type: "u32" }, { name: "version", type: "u32" }, { name: "payloadPtr", type: "usize" }, { name: "payloadLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_copy_persisted_state",
    importName: "fui_copy_persisted_state",
    args: [{ name: "nodeIdPtr", type: "usize" }, { name: "nodeIdLen", type: "u32" }, { name: "kindPtr", type: "usize" }, { name: "kindLen", type: "u32" }, { name: "outVersionPtr", type: "usize" }, { name: "payloadPtr", type: "usize" }, { name: "payloadCapacity", type: "u32" }],
    returns: "i32"
  },
  {
    name: "fui_log",
    importName: "fui_log",
    args: [{ name: "categoryPtr", type: "usize" }, { name: "catLen", type: "u32" }, { name: "msgPtr", type: "usize" }, { name: "msgLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_logs_enabled",
    importName: "fui_logs_enabled",
    args: [],
    returns: "bool"
  },
  {
    name: "fui_worker_start_string",
    importName: "fui_worker_start_string",
    args: [{ name: "workerId", type: "u32" }, { name: "entryPtr", type: "usize" }, { name: "entryLen", type: "u32" }, { name: "inputPtr", type: "usize" }, { name: "inputLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_worker_cancel",
    importName: "fui_worker_cancel",
    args: [{ name: "workerId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_capabilities",
    importName: "fui_file_capabilities",
    args: [],
    returns: "u32"
  },
  {
    name: "fui_file_pick",
    importName: "fui_file_pick",
    args: [{ name: "requestId", type: "u32" }, { name: "acceptPtr", type: "usize" }, { name: "acceptLen", type: "u32" }, { name: "multiple", type: "bool" }],
    returns: "void"
  },
  {
    name: "fui_file_read_chunk",
    importName: "fui_file_read_chunk",
    args: [{ name: "requestId", type: "u32" }, { name: "fileIdPtr", type: "usize" }, { name: "fileIdLen", type: "u32" }, { name: "offsetBytes", type: "u64" }, { name: "maxBytes", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_save_text",
    importName: "fui_file_save_text",
    args: [{ name: "requestId", type: "u32" }, { name: "suggestedNamePtr", type: "usize" }, { name: "suggestedNameLen", type: "u32" }, { name: "mimeTypePtr", type: "usize" }, { name: "mimeTypeLen", type: "u32" }, { name: "fileExtensionPtr", type: "usize" }, { name: "fileExtensionLen", type: "u32" }, { name: "textPtr", type: "usize" }, { name: "textLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_save_bytes",
    importName: "fui_file_save_bytes",
    args: [{ name: "requestId", type: "u32" }, { name: "suggestedNamePtr", type: "usize" }, { name: "suggestedNameLen", type: "u32" }, { name: "mimeTypePtr", type: "usize" }, { name: "mimeTypeLen", type: "u32" }, { name: "fileExtensionPtr", type: "usize" }, { name: "fileExtensionLen", type: "u32" }, { name: "bytesPtr", type: "usize" }, { name: "bytesLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_create_writer",
    importName: "fui_file_create_writer",
    args: [{ name: "requestId", type: "u32" }, { name: "suggestedNamePtr", type: "usize" }, { name: "suggestedNameLen", type: "u32" }, { name: "mimeTypePtr", type: "usize" }, { name: "mimeTypeLen", type: "u32" }, { name: "fileExtensionPtr", type: "usize" }, { name: "fileExtensionLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_writer_write_text",
    importName: "fui_file_writer_write_text",
    args: [{ name: "requestId", type: "u32" }, { name: "writerIdPtr", type: "usize" }, { name: "writerIdLen", type: "u32" }, { name: "textPtr", type: "usize" }, { name: "textLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_writer_write_bytes",
    importName: "fui_file_writer_write_bytes",
    args: [{ name: "requestId", type: "u32" }, { name: "writerIdPtr", type: "usize" }, { name: "writerIdLen", type: "u32" }, { name: "bytesPtr", type: "usize" }, { name: "bytesLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_writer_finish",
    importName: "fui_file_writer_finish",
    args: [{ name: "requestId", type: "u32" }, { name: "writerIdPtr", type: "usize" }, { name: "writerIdLen", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_file_process_worker_start",
    importName: "fui_file_process_worker_start",
    args: [{ name: "requestId", type: "u32" }, { name: "fileIdPtr", type: "usize" }, { name: "fileIdLen", type: "u32" }, { name: "suggestedNamePtr", type: "usize" }, { name: "suggestedNameLen", type: "u32" }, { name: "chunkBytes", type: "u32" }, { name: "saveToPickedFile", type: "bool" }],
    returns: "void"
  },
  {
    name: "fui_file_process_worker_cancel",
    importName: "fui_file_process_worker_cancel",
    args: [{ name: "requestId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_save",
    importName: "fui_canvas_save",
    args: [{ name: "canvasPtr", type: "usize" }],
    returns: "void"
  },
  {
    name: "fui_canvas_restore",
    importName: "fui_canvas_restore",
    args: [{ name: "canvasPtr", type: "usize" }],
    returns: "void"
  },
  {
    name: "fui_canvas_translate",
    importName: "fui_canvas_translate",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_scale",
    importName: "fui_canvas_scale",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "sx", type: "f32" }, { name: "sy", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_rotate",
    importName: "fui_canvas_rotate",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "degrees", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_clip_rect",
    importName: "fui_canvas_clip_rect",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_clip_round_rect",
    importName: "fui_canvas_clip_round_rect",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }, { name: "topLeft", type: "f32" }, { name: "topRight", type: "f32" }, { name: "bottomRight", type: "f32" }, { name: "bottomLeft", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_rect",
    importName: "fui_canvas_draw_rect",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }, { name: "fillColor", type: "u32" }, { name: "strokeColor", type: "u32" }, { name: "strokeWidth", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_circle",
    importName: "fui_canvas_draw_circle",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "cx", type: "f32" }, { name: "cy", type: "f32" }, { name: "radius", type: "f32" }, { name: "fillColor", type: "u32" }, { name: "strokeColor", type: "u32" }, { name: "strokeWidth", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_line",
    importName: "fui_canvas_draw_line",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "x1", type: "f32" }, { name: "y1", type: "f32" }, { name: "x2", type: "f32" }, { name: "y2", type: "f32" }, { name: "color", type: "u32" }, { name: "strokeWidth", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_round_rect",
    importName: "fui_canvas_draw_round_rect",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }, { name: "rx", type: "f32" }, { name: "ry", type: "f32" }, { name: "fillColor", type: "u32" }, { name: "strokeColor", type: "u32" }, { name: "strokeWidth", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_path_create",
    importName: "fui_path_create",
    args: [],
    returns: "u32"
  },
  {
    name: "fui_path_destroy",
    importName: "fui_path_destroy",
    args: [{ name: "pathId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_path_move_to",
    importName: "fui_path_move_to",
    args: [{ name: "pathId", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_path_line_to",
    importName: "fui_path_line_to",
    args: [{ name: "pathId", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_path_quad_to",
    importName: "fui_path_quad_to",
    args: [{ name: "pathId", type: "u32" }, { name: "cx", type: "f32" }, { name: "cy", type: "f32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_path_cubic_to",
    importName: "fui_path_cubic_to",
    args: [{ name: "pathId", type: "u32" }, { name: "cx1", type: "f32" }, { name: "cy1", type: "f32" }, { name: "cx2", type: "f32" }, { name: "cy2", type: "f32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_path_close",
    importName: "fui_path_close",
    args: [{ name: "pathId", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_path_add_rect",
    importName: "fui_path_add_rect",
    args: [{ name: "pathId", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_path_add_circle",
    importName: "fui_path_add_circle",
    args: [{ name: "pathId", type: "u32" }, { name: "cx", type: "f32" }, { name: "cy", type: "f32" }, { name: "r", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_path",
    importName: "fui_canvas_draw_path",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "pathId", type: "u32" }, { name: "fillColor", type: "u32" }, { name: "strokeColor", type: "u32" }, { name: "strokeWidth", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_text_node",
    importName: "fui_canvas_draw_text_node",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "handleLo", type: "u32" }, { name: "handleHi", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_image",
    importName: "fui_canvas_draw_image",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "textureId", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }, { name: "samplingKind", type: "u32" }, { name: "maxAniso", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_svg",
    importName: "fui_canvas_draw_svg",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "svgId", type: "u32" }, { name: "x", type: "f32" }, { name: "y", type: "f32" }, { name: "w", type: "f32" }, { name: "h", type: "f32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_draw_batch",
    importName: "fui_canvas_draw_batch",
    args: [{ name: "canvasPtr", type: "usize" }, { name: "wordsPtr", type: "usize" }, { name: "wordCount", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_create_offscreen",
    importName: "fui_canvas_create_offscreen",
    args: [{ name: "width", type: "u32" }, { name: "height", type: "u32" }],
    returns: "u32"
  },
  {
    name: "fui_canvas_get_offscreen_ptr",
    importName: "fui_canvas_get_offscreen_ptr",
    args: [{ name: "offscreenId", type: "u32" }],
    returns: "usize"
  },
  {
    name: "fui_canvas_read_offscreen_pixels",
    importName: "fui_canvas_read_offscreen_pixels",
    args: [{ name: "offscreenId", type: "u32" }, { name: "outPtr", type: "usize" }, { name: "width", type: "u32" }, { name: "height", type: "u32" }],
    returns: "void"
  },
  {
    name: "fui_canvas_destroy_offscreen",
    importName: "fui_canvas_destroy_offscreen",
    args: [{ name: "offscreenId", type: "u32" }],
    returns: "void"
  }
];

// v2/abi/fui-as/strategies/common.ts
function parseFuiAsUiOptions(context) {
  const packageDir = path3.join(context.repoRoot, "v2/fui-as");
  return {
    headerPath: path3.resolve(getArg(context.args, "header") ?? path3.join(context.repoRoot, "v2/ui/include/effindom_ui.h")),
    outputPath: path3.resolve(getArg(context.args, "out") ?? path3.join(packageDir, "src/core/generated/UiAbi.ts")),
    usageSourcePath: path3.resolve(getArg(context.args, "usage-source") ?? path3.join(packageDir, "src"))
  };
}
function parseFuiAsGeneratedFileOptions(context, defaultOutputRelative) {
  const packageDir = path3.join(context.repoRoot, "v2/fui-as");
  return {
    outputPath: path3.resolve(getArg(context.args, "out") ?? path3.join(packageDir, defaultOutputRelative)),
    usageSourcePath: path3.resolve(getArg(context.args, "usage-source") ?? path3.join(packageDir, "src"))
  };
}
async function collectUsedHostImports(sourcePath, outputPath) {
  return collectUsedNamedImports(fuiHostImports, sourcePath, outputPath);
}
async function collectUsedNamedImports(imports, sourcePath, outputPath) {
  const allHostNames = new Set(imports.map((item) => item.name));
  const used = /* @__PURE__ */ new Set();
  async function visit(currentPath) {
    const entries = await fs3.readdir(currentPath, { withFileTypes: true });
    await Promise.all(entries.map(async (entry) => {
      const entryPath = path3.join(currentPath, entry.name);
      if (entry.isDirectory()) {
        if (entry.name === "generated" || entry.name === "build" || entry.name === "node_modules") {
          return;
        }
        await visit(entryPath);
        return;
      }
      if (!entry.isFile() || !entry.name.endsWith(".ts") || entryPath === outputPath) {
        return;
      }
      const text = await fs3.readFile(entryPath, "utf8");
      for (const match of text.matchAll(/\bffi\.([A-Za-z_]\w*)\b/g)) {
        const name = match[1];
        if (allHostNames.has(name)) {
          used.add(name);
        }
      }
      for (const match of text.matchAll(/import\s*\{([^}]+)\}\s*from\s*["'][^"']*\/?ffi["'];/g)) {
        for (const rawName of match[1].split(",")) {
          const name = rawName.trim().split(/\s+as\s+/)[0].trim();
          if (allHostNames.has(name)) {
            used.add(name);
          }
        }
      }
    }));
  }
  await visit(sourcePath);
  return used;
}

// v2/abi/fui-as/strategies/ui.ts
function emitFuiAsUiFile(packageDir, headerPath, functions) {
  const blocks = [
    "// @ts-nocheck",
    `// Generated by v2/abi/generate.ts strategy fui-as-ui from ${sourcePathForHeader(packageDir, headerPath)}.`,
    "// Includes only raw UI ABI imports referenced by v2/fui-as Tier 3 code.",
    "// Public SDK-facing enum aliases live in core/ffi.ts.",
    "// Do not edit by hand.",
    "",
    functions.map((fn) => emitAssemblyScriptExternal("effindom_v2_ui", fn)).join("\n\n"),
    ""
  ];
  return blocks.join("\n").replace(/\n{3,}/g, "\n\n");
}
var FuiAsUiStrategy = class {
  name = "fui-as-ui";
  async run(context) {
    const options = parseFuiAsUiOptions(context);
    const source = await fs4.readFile(options.headerPath, "utf8");
    const usedFunctions = await collectUsedAssemblyScriptFfiImports(
      options.usageSourcePath,
      options.outputPath,
      "ui_"
    );
    const functions = selectUsedFunctions(parseCHeader(source).functions, usedFunctions);
    const packageDir = path4.join(context.repoRoot, "v2/fui-as");
    const generated = emitFuiAsUiFile(packageDir, options.headerPath, functions);
    await writeGeneratedFile(
      options.outputPath,
      generated,
      context.check,
      `${path4.relative(process.cwd(), options.outputPath)} is stale. Run npm run generate:ui-abi from v2/fui-as.`
    );
  }
};

// v2/abi/fui-as/strategies/host.ts
function emitFuiAsHostFile(functions) {
  const blocks = [
    "// @ts-nocheck",
    "// Generated by v2/abi/generate.ts strategy fui-as-host from v2/abi/shared/fui-host.ts.",
    "// Includes only raw fui_host imports referenced by v2/fui-as Tier 3 code.",
    "// Do not edit by hand.",
    "",
    functions.map((item) => emitAssemblyScriptHostExternal("fui_host", item)).join("\n\n"),
    ""
  ];
  return blocks.join("\n").replace(/\n{3,}/g, "\n\n");
}
var FuiAsHostStrategy = class {
  name = "fui-as-host";
  async run(context) {
    const options = parseFuiAsGeneratedFileOptions(context, "src/core/generated/HostAbi.ts");
    const usedFunctions = await collectUsedHostImports(options.usageSourcePath, options.outputPath);
    const functions = fuiHostImports.filter((item) => usedFunctions.has(item.name));
    const generated = emitFuiAsHostFile(functions);
    await writeGeneratedFile(
      options.outputPath,
      generated,
      context.check,
      `${options.outputPath} is stale. Run npm run generate:abi from v2/fui-as.`
    );
  }
};

// v2/abi/shared/fui-fetch-host.ts
var fuiFetchHostImports = [
  {
    name: "fui_fetch_start",
    importName: "fui_fetch_start",
    args: [
      { name: "requestId", type: "u32" },
      { name: "methodPtr", type: "usize" },
      { name: "methodLen", type: "u32" },
      { name: "urlPtr", type: "usize" },
      { name: "urlLen", type: "u32" },
      { name: "headersPtr", type: "usize" },
      { name: "headersLen", type: "u32" },
      { name: "bodyPtr", type: "usize" },
      { name: "bodyLen", type: "u32" }
    ],
    returns: "void"
  },
  {
    name: "fui_fetch_cancel",
    importName: "fui_fetch_cancel",
    args: [{ name: "requestId", type: "u32" }],
    returns: "void"
  }
];

// v2/abi/fui-as/strategies/fetch-host.ts
function emitFuiAsFetchHostFile(functions) {
  const blocks = [
    "// @ts-nocheck",
    "// Generated by v2/abi/generate.ts strategy fui-as-fetch-host from v2/abi/shared/fui-fetch-host.ts.",
    "// Includes only raw fui_fetch_host imports referenced by v2/fui-as Tier 3 code.",
    "// Do not edit by hand.",
    "",
    functions.map((item) => emitAssemblyScriptHostExternal("fui_fetch_host", item)).join("\n\n"),
    ""
  ];
  return blocks.join("\n").replace(/\n{3,}/g, "\n\n");
}
var FuiAsFetchHostStrategy = class {
  name = "fui-as-fetch-host";
  async run(context) {
    const options = parseFuiAsGeneratedFileOptions(context, "src/core/generated/FetchHostAbi.ts");
    const generated = emitFuiAsFetchHostFile(fuiFetchHostImports);
    await writeGeneratedFile(
      options.outputPath,
      generated,
      context.check,
      `${options.outputPath} is stale. Run npm run generate:abi from v2/fui-as.`
    );
  }
};

// v2/abi/fui-as/strategies/enums.ts
import { promises as fs5 } from "node:fs";
import path5 from "node:path";

// v2/abi/shared/enum-generation.ts
function valuesForEnum(source, enumName) {
  const found = source.enums.find((item) => item.name === enumName);
  if (found === void 0) {
    throw new Error(`Could not find ABI enum ${enumName}.`);
  }
  return new Map(found.members.map((member) => [member.name, member.value]));
}
function valuesForHostEnum(enumName) {
  const found = fuiHostEnums.find((item) => item.name === enumName);
  if (found === void 0) {
    throw new Error(`Could not find host ABI enum ${enumName}.`);
  }
  return new Map(found.members.map((member) => [member.name, member.value]));
}
function valuesForEnumSpec(spec, uiSource, coreSource) {
  if (spec.source === "constant") {
    return new Map(uiSource.constants.map((constant) => [constant.name, constant.value]));
  }
  if (spec.source === "host") {
    if (spec.sourceEnum === void 0) {
      throw new Error(`Generated enum ${spec.name} must define sourceEnum.`);
    }
    return valuesForHostEnum(spec.sourceEnum);
  }
  if (spec.sourceEnum === void 0) {
    throw new Error(`Generated enum ${spec.name} must define sourceEnum.`);
  }
  return valuesForEnum(spec.source === "ui" ? uiSource : coreSource, spec.sourceEnum);
}
function emitTypeScriptEnum(spec, values) {
  return [
    `export enum ${spec.name} {`,
    ...spec.members.map((member) => {
      const value = values.get(member.source);
      if (value === void 0) {
        throw new Error(`Could not find ABI enum member ${member.source} for ${spec.name}.${member.name}.`);
      }
      return `  ${member.name} = ${value},`;
    }),
    "}"
  ].join("\n");
}
function emitRustEnum(spec, values, backing) {
  return [
    `#[repr(${backing})]`,
    "#[derive(Debug, Clone, Copy, PartialEq, Eq)]",
    `pub enum ${spec.name} {`,
    ...spec.members.map((member) => {
      const value = values.get(member.source);
      if (value === void 0) {
        throw new Error(`Could not find ABI enum member ${member.source} for ${spec.name}.${member.name}.`);
      }
      return `    ${member.name} = ${value},`;
    }),
    "}"
  ].join("\n");
}

// v2/abi/shared/model/enum-specs.ts
var canonicalEnumSpecs = [
  { name: "HandleValue", source: "constant", members: [{ name: "Invalid", source: "UI_INVALID_HANDLE" }] },
  {
    name: "NodeType",
    source: "ui",
    sourceEnum: "UiNodeType",
    members: [
      { name: "FlexBox", source: "UI_NODE_FLEX_BOX" },
      { name: "Text", source: "UI_NODE_TEXT" },
      { name: "Image", source: "UI_NODE_IMAGE" },
      { name: "Svg", source: "UI_NODE_SVG" },
      { name: "ScrollView", source: "UI_NODE_SCROLLVIEW" },
      { name: "Grid", source: "UI_NODE_GRID" }
    ]
  },
  {
    name: "Unit",
    source: "ui",
    sourceEnum: "UiSizeUnit",
    members: [
      { name: "Pixel", source: "UI_SIZE_UNIT_PIXEL" },
      { name: "Auto", source: "UI_SIZE_UNIT_AUTO" },
      { name: "Percent", source: "UI_SIZE_UNIT_PERCENT" }
    ]
  },
  {
    name: "GridUnit",
    source: "ui",
    sourceEnum: "UiGridUnit",
    members: [
      { name: "Pixel", source: "UI_GRID_UNIT_PIXEL" },
      { name: "Auto", source: "UI_GRID_UNIT_AUTO" },
      { name: "Star", source: "UI_GRID_UNIT_STAR" }
    ]
  },
  {
    name: "PositionType",
    source: "ui",
    sourceEnum: "UiPositionType",
    members: [
      { name: "Relative", source: "UI_POSITION_RELATIVE" },
      { name: "Absolute", source: "UI_POSITION_ABSOLUTE" }
    ]
  },
  {
    name: "Visibility",
    source: "ui",
    sourceEnum: "UiVisibility",
    members: [
      { name: "Normal", source: "UI_VISIBILITY_NORMAL" },
      { name: "Hidden", source: "UI_VISIBILITY_HIDDEN" },
      { name: "Collapsed", source: "UI_VISIBILITY_COLLAPSED" }
    ]
  },
  {
    name: "FlexDirection",
    source: "ui",
    sourceEnum: "UiFlexDirection",
    members: [
      { name: "Column", source: "UI_FLEX_DIRECTION_COLUMN" },
      { name: "Row", source: "UI_FLEX_DIRECTION_ROW" }
    ]
  },
  {
    name: "FlexWrap",
    source: "ui",
    sourceEnum: "UiFlexWrap",
    members: [
      { name: "NoWrap", source: "UI_FLEX_WRAP_NO_WRAP" },
      { name: "Wrap", source: "UI_FLEX_WRAP_WRAP" },
      { name: "WrapReverse", source: "UI_FLEX_WRAP_WRAP_REVERSE" }
    ]
  },
  {
    name: "JustifyContent",
    source: "ui",
    sourceEnum: "UiJustifyContent",
    members: [
      { name: "Start", source: "UI_JUSTIFY_START" },
      { name: "Center", source: "UI_JUSTIFY_CENTER" },
      { name: "End", source: "UI_JUSTIFY_END" }
    ]
  },
  {
    name: "AlignItems",
    source: "ui",
    sourceEnum: "UiAlignItems",
    members: [
      { name: "Start", source: "UI_ALIGN_ITEMS_START" },
      { name: "Center", source: "UI_ALIGN_ITEMS_CENTER" },
      { name: "End", source: "UI_ALIGN_ITEMS_END" },
      { name: "Stretch", source: "UI_ALIGN_ITEMS_STRETCH" },
      { name: "None", source: "UI_ALIGN_ITEMS_NONE" }
    ]
  },
  {
    name: "AlignSelf",
    source: "ui",
    sourceEnum: "UiAlignSelf",
    members: [
      { name: "Auto", source: "UI_ALIGN_SELF_AUTO" },
      { name: "Start", source: "UI_ALIGN_SELF_START" },
      { name: "Center", source: "UI_ALIGN_SELF_CENTER" },
      { name: "End", source: "UI_ALIGN_SELF_END" },
      { name: "Stretch", source: "UI_ALIGN_SELF_STRETCH" }
    ]
  },
  {
    name: "BorderStyle",
    source: "core",
    sourceEnum: "EdBorderStyle",
    members: [
      { name: "Solid", source: "ED_BORDER_SOLID" },
      { name: "Dashed", source: "ED_BORDER_DASHED" },
      { name: "Dotted", source: "ED_BORDER_DOTTED" }
    ]
  },
  {
    name: "ObjectFit",
    source: "core",
    sourceEnum: "EdObjectFit",
    members: [
      { name: "Fill", source: "ED_OBJECT_FIT_FILL" },
      { name: "Contain", source: "ED_OBJECT_FIT_CONTAIN" },
      { name: "Cover", source: "ED_OBJECT_FIT_COVER" },
      { name: "None", source: "ED_OBJECT_FIT_NONE" },
      { name: "ScaleDown", source: "ED_OBJECT_FIT_SCALE_DOWN" }
    ]
  },
  {
    name: "ImageSamplingKind",
    source: "core",
    sourceEnum: "EdImageSampling",
    members: [
      { name: "Linear", source: "ED_IMAGE_SAMPLING_LINEAR" },
      { name: "Nearest", source: "ED_IMAGE_SAMPLING_NEAREST" },
      { name: "LinearMipmapNearest", source: "ED_IMAGE_SAMPLING_LINEAR_MIPMAP_NEAREST" },
      { name: "LinearMipmapLinear", source: "ED_IMAGE_SAMPLING_LINEAR_MIPMAP_LINEAR" },
      { name: "CubicMitchell", source: "ED_IMAGE_SAMPLING_CUBIC_MITCHELL" },
      { name: "CubicCatmullRom", source: "ED_IMAGE_SAMPLING_CUBIC_CATMULL_ROM" },
      { name: "Anisotropic", source: "ED_IMAGE_SAMPLING_ANISOTROPIC" }
    ]
  },
  {
    name: "TextAlign",
    source: "ui",
    sourceEnum: "UiTextAlign",
    members: [
      { name: "Left", source: "UI_TEXT_ALIGN_LEFT" },
      { name: "Center", source: "UI_TEXT_ALIGN_CENTER" },
      { name: "Right", source: "UI_TEXT_ALIGN_RIGHT" }
    ]
  },
  {
    name: "TextVerticalAlign",
    source: "ui",
    sourceEnum: "UiTextVerticalAlign",
    members: [
      { name: "Top", source: "UI_TEXT_VERTICAL_ALIGN_TOP" },
      { name: "Center", source: "UI_TEXT_VERTICAL_ALIGN_CENTER" },
      { name: "Bottom", source: "UI_TEXT_VERTICAL_ALIGN_BOTTOM" }
    ]
  },
  {
    name: "TextOverflow",
    source: "ui",
    sourceEnum: "UiTextOverflow",
    members: [
      { name: "Clip", source: "UI_TEXT_OVERFLOW_CLIP" },
      { name: "Ellipsis", source: "UI_TEXT_OVERFLOW_ELLIPSIS" },
      { name: "Fade", source: "UI_TEXT_OVERFLOW_FADE" }
    ]
  },
  {
    name: "Orientation",
    source: "ui",
    sourceEnum: "UiOrientation",
    members: [
      { name: "None", source: "UI_ORIENTATION_NONE" },
      { name: "Horizontal", source: "UI_ORIENTATION_HORIZONTAL" },
      { name: "Vertical", source: "UI_ORIENTATION_VERTICAL" }
    ]
  },
  {
    name: "KeyEventType",
    source: "ui",
    sourceEnum: "UiKeyEventType",
    members: [
      { name: "Down", source: "UI_KEY_EVENT_DOWN" },
      { name: "Up", source: "UI_KEY_EVENT_UP" }
    ]
  },
  {
    name: "PointerEventType",
    source: "ui",
    sourceEnum: "UiEvent",
    members: [
      { name: "Down", source: "UI_EVENT_POINTER_DOWN" },
      { name: "Up", source: "UI_EVENT_POINTER_UP" },
      { name: "Move", source: "UI_EVENT_POINTER_MOVE" },
      { name: "Enter", source: "UI_EVENT_POINTER_ENTER" },
      { name: "Leave", source: "UI_EVENT_POINTER_LEAVE" },
      { name: "Cancel", source: "UI_EVENT_POINTER_CANCEL" }
    ]
  },
  {
    name: "KeyModifier",
    source: "ui",
    sourceEnum: "UiKeyModifier",
    members: [
      { name: "Shift", source: "UI_KEY_MOD_SHIFT" },
      { name: "Ctrl", source: "UI_KEY_MOD_CTRL" },
      { name: "Alt", source: "UI_KEY_MOD_ALT" },
      { name: "Meta", source: "UI_KEY_MOD_META" }
    ]
  },
  {
    name: "SemanticRole",
    source: "ui",
    sourceEnum: "UiSemanticRole",
    members: [
      { name: "None", source: "UI_SEMANTIC_NONE" },
      { name: "Button", source: "UI_SEMANTIC_BUTTON" },
      { name: "Textbox", source: "UI_SEMANTIC_TEXTBOX" },
      { name: "Link", source: "UI_SEMANTIC_LINK" },
      { name: "Heading", source: "UI_SEMANTIC_HEADING" },
      { name: "Form", source: "UI_SEMANTIC_FORM" },
      { name: "List", source: "UI_SEMANTIC_LIST" },
      { name: "ListItem", source: "UI_SEMANTIC_LIST_ITEM" },
      { name: "Image", source: "UI_SEMANTIC_IMAGE" },
      { name: "Dialog", source: "UI_SEMANTIC_DIALOG" },
      { name: "StaticText", source: "UI_SEMANTIC_STATIC_TEXT" },
      { name: "Checkbox", source: "UI_SEMANTIC_CHECKBOX" },
      { name: "Radio", source: "UI_SEMANTIC_RADIO" },
      { name: "RadioGroup", source: "UI_SEMANTIC_RADIO_GROUP" },
      { name: "Switch", source: "UI_SEMANTIC_SWITCH" },
      { name: "Slider", source: "UI_SEMANTIC_SLIDER" },
      { name: "ComboBox", source: "UI_SEMANTIC_COMBOBOX" }
    ]
  },
  {
    name: "CursorStyle",
    source: "host",
    sourceEnum: "FuiCursorStyle",
    members: [
      { name: "Default", source: "FUI_CURSOR_DEFAULT" },
      { name: "Pointer", source: "FUI_CURSOR_POINTER" },
      { name: "Text", source: "FUI_CURSOR_TEXT" },
      { name: "Move", source: "FUI_CURSOR_MOVE" },
      { name: "Grab", source: "FUI_CURSOR_GRAB" },
      { name: "Grabbing", source: "FUI_CURSOR_GRABBING" },
      { name: "ResizeNS", source: "FUI_CURSOR_RESIZE_NS" },
      { name: "ResizeEW", source: "FUI_CURSOR_RESIZE_EW" }
    ]
  },
  {
    name: "SemanticCheckedState",
    source: "ui",
    sourceEnum: "UiSemanticCheckedState",
    members: [
      { name: "None", source: "UI_SEMANTIC_CHECKED_NONE" },
      { name: "False", source: "UI_SEMANTIC_CHECKED_FALSE" },
      { name: "True", source: "UI_SEMANTIC_CHECKED_TRUE" },
      { name: "Mixed", source: "UI_SEMANTIC_CHECKED_MIXED" }
    ]
  }
];

// v2/abi/fui-as/strategies/enums.ts
function emitFuiAsEnumsFile(uiSource, coreSource) {
  const blocks = [
    "// @ts-nocheck",
    "// Generated by v2/abi/generate.ts strategy fui-as-enums from v2/ui/include/effindom_ui.h and v2/core/include/effindom.h.",
    "// Includes SDK facade enum names backed by canonical ABI header values.",
    "// SDK-only facade enums remain in core/ffi.ts until they have a canonical ABI source.",
    "// Do not edit by hand.",
    "",
    ...canonicalEnumSpecs.map((spec) => emitTypeScriptEnum(spec, valuesForEnumSpec(spec, uiSource, coreSource))),
    ""
  ];
  return blocks.join("\n\n").replace(/\n{3,}/g, "\n\n");
}
var FuiAsEnumsStrategy = class {
  name = "fui-as-enums";
  async run(context) {
    const options = parseFuiAsGeneratedFileOptions(context, "src/core/generated/UiEnums.ts");
    const uiSource = parseCHeader(await fs5.readFile(path5.join(context.repoRoot, "v2/ui/include/effindom_ui.h"), "utf8"));
    const coreSource = parseCHeader(await fs5.readFile(path5.join(context.repoRoot, "v2/core/include/effindom.h"), "utf8"));
    const generated = emitFuiAsEnumsFile(uiSource, coreSource);
    await writeGeneratedFile(
      options.outputPath,
      generated,
      context.check,
      `${options.outputPath} is stale. Run npm run generate:abi from v2/fui-as.`
    );
  }
};

// v2/abi/fui-rs/strategies/ffi.ts
import { promises as fs6 } from "node:fs";
import path6 from "node:path";

// v2/abi/fui-rs/render/names.ts
function rustParamName(name) {
  switch (name) {
    case "type":
    case "match":
    case "ref":
    case "self":
    case "super":
    case "crate":
    case "mod":
    case "move":
    case "box":
    case "use":
    case "where":
      return `${name}_`;
    default:
      return name.replace(/[^A-Za-z0-9_]/g, "_");
  }
}

// v2/abi/fui-rs/render/types.ts
function rustType(cType) {
  const raw = normalizeCType(cType);
  const isConstPointer = raw.startsWith("const ") && raw.includes("*");
  const normalized = raw.replace(/^const\s+/, "");
  if (normalized.includes("*")) {
    const pointee = rustScalarCType(normalized.replace(/\*/g, "").trim());
    return `${isConstPointer ? "*const" : "*mut"} ${pointee}`;
  }
  return rustScalarCType(normalized);
}
function rustScalarCType(normalized) {
  switch (normalized) {
    case "void":
      return "()";
    case "bool":
      return "bool";
    case "float":
      return "f32";
    case "double":
      return "f64";
    case "int32_t":
      return "i32";
    case "uint32_t":
    case "ui_color_t":
      return "u32";
    case "uint8_t":
      return "u8";
    case "uint64_t":
    case "ui_handle_t":
      return "u64";
    case "uintptr_t":
      return "usize";
    default:
      if (normalized.startsWith("Ui") || normalized.startsWith("Ed")) {
        return "u32";
      }
      throw new Error(`Unsupported C ABI type for Rust: ${normalized}`);
  }
}
function rustScalarType(type) {
  switch (type) {
    case "void":
      return "()";
    case "bool":
      return "bool";
    case "i32":
      return "i32";
    case "u32":
      return "u32";
    case "u64":
      return "u64";
    case "f32":
      return "f32";
    case "usize":
      return "usize";
  }
}
function rustReturnType(returnType) {
  const type = rustType(returnType);
  return type === "()" ? "" : ` -> ${type}`;
}
function rustHostReturnType(returnType) {
  const type = rustScalarType(returnType);
  return type === "()" ? "" : ` -> ${type}`;
}
function rustEnumBacking(spec) {
  return spec.name === "HandleValue" ? "u64" : "u32";
}

// v2/abi/fui-rs/render/imports.ts
function emitRustParamList(params) {
  return params.map((param) => `${rustParamName(param.name)}: ${rustType(param.type)}`).join(", ");
}
function emitRustHostParamList(params) {
  return params.map((param) => `${rustParamName(param.name)}: ${rustScalarType(param.type)}`).join(", ");
}
function emitRustWasmUiImport(fn) {
  const params = fn.name === "ui_commit_frame" ? "" : emitRustParamList(fn.params);
  const returnType = rustReturnType(fn.returnType);
  return `    pub fn ${fn.name}(${params})${returnType};`;
}
function emitRustWasmHostImport(item) {
  return `    pub fn ${item.name}(${emitRustHostParamList(item.args)})${rustHostReturnType(item.returns)};`;
}

// v2/abi/fui-rs/render/file.ts
function emitFuiRsFfiFile(uiSource, coreSource, uiFunctions) {
  const uiImports = uiFunctions.filter((fn) => fn.name.startsWith("ui_"));
  const blocks = [
    "// Generated by v2/abi/generate.ts strategy fui-rs-ffi.",
    "// Raw ABI imports, ABI enums, and non-wasm test doubles for FUI-RS.",
    "// Do not edit by hand.",
    "",
    "#![allow(dead_code)]",
    "#![allow(non_snake_case)]",
    "#![allow(clippy::too_many_arguments)]",
    "#![allow(clippy::missing_safety_doc)]",
    "",
    ...canonicalEnumSpecs.map((spec) => emitRustEnum(spec, valuesForEnumSpec(spec, uiSource, coreSource), rustEnumBacking(spec))),
    "",
    '#[cfg(target_arch = "wasm32")]',
    '#[link(wasm_import_module = "effindom_v2_ui")]',
    'unsafe extern "C" {',
    uiImports.map(emitRustWasmUiImport).join("\n"),
    "}",
    "",
    '#[cfg(target_arch = "wasm32")]',
    '#[link(wasm_import_module = "fui_host")]',
    'unsafe extern "C" {',
    fuiHostImports.map(emitRustWasmHostImport).join("\n"),
    "}",
    "",
    '#[cfg(target_arch = "wasm32")]',
    '#[link(wasm_import_module = "fui_fetch_host")]',
    'unsafe extern "C" {',
    fuiFetchHostImports.map(emitRustWasmHostImport).join("\n"),
    "}",
    ""
  ];
  return blocks.join("\n").replace(/\n{3,}/g, "\n\n");
}

// v2/abi/fui-rs/strategies/ffi.ts
var FuiRsFfiStrategy = class {
  name = "fui-rs-ffi";
  async run(context) {
    const outputPath = path6.resolve(getArg(context.args, "out") ?? path6.join(context.repoRoot, "v2/fui-rs/src/generated/ffi.rs"));
    const uiHeaderPath = path6.join(context.repoRoot, "v2/ui/include/effindom_ui.h");
    const coreHeaderPath = path6.join(context.repoRoot, "v2/core/include/effindom.h");
    const uiSource = parseCHeader(await fs6.readFile(uiHeaderPath, "utf8"));
    const coreSource = parseCHeader(await fs6.readFile(coreHeaderPath, "utf8"));
    const generated = emitFuiRsFfiFile(uiSource, coreSource, uiSource.functions);
    await writeGeneratedFile(
      outputPath,
      generated,
      context.check,
      `${path6.relative(process.cwd(), outputPath)} is stale. Run npm run generate:abi from v2/fui-rs.`
    );
  }
};

// v2/abi/strategies.ts
var strategies = [
  new FuiAsUiStrategy(),
  new FuiAsHostStrategy(),
  new FuiAsFetchHostStrategy(),
  new FuiAsEnumsStrategy(),
  new FuiRsFfiStrategy()
];

// v2/abi/generate.ts
async function main() {
  const { strategyName, check, rest } = parseCommand(process.argv.slice(2), strategies);
  const strategy = strategies.find((candidate) => candidate.name === strategyName);
  if (strategy === void 0) {
    throw new Error(`Unknown generator strategy "${strategyName}". Available strategies: ${strategies.map((item) => item.name).join(", ")}`);
  }
  await strategy.run({
    repoRoot: await findRepoRoot(process.cwd()),
    check,
    args: rest
  });
}
main().catch((error) => {
  console.error(error instanceof Error ? error.message : String(error));
  process.exitCode = 1;
});
