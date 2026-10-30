// Generates the per-type API reference under docs/v2/fui-cs/reference from the XML documentation and sources of
// the SDK and its worker library: the XML gives each documented member's summary, the sources give each type's
// namespace, kind, folder and base types (to resolve <inheritdoc/>) and the generated enums' members, which carry
// no XML docs. A type's page is named after its namespace below FuiCs, so Workers.FetchRequest.md sits beside
// FetchRequest.md.
//
// Usage: generate-api-reference <output directory> <FuiCs.xml> <src/FuiCs> <FuiCs.Worker.xml> <src/FuiCs.Worker> [--check]

import { promises as fs } from 'node:fs';
import path from 'node:path';

import { parseCommandLine } from './hostgen-cli.ts';

const USAGE = 'Usage: generate-api-reference <output directory> <FuiCs.xml> <src/FuiCs> <FuiCs.Worker.xml> <src/FuiCs.Worker> [--check]';

interface SourceType {
  /** The type's name below the FuiCs namespace, with its generic arity: `Button`, `Workers.WorkerJob`, `Signal`1`. */
  readonly name: string;
  /** The namespace below FuiCs, empty for FuiCs itself. */
  readonly namespace: string;
  readonly displayName: string;
  readonly kind: string;
  readonly folder: string;
  readonly bases: readonly string[];
  readonly enumMembers: readonly string[];
}

interface XmlMember {
  readonly id: string;
  readonly body: string;
}

interface Entry {
  readonly kind: string;
  readonly signature: string;
  readonly text: string;
}

const AREAS: readonly (readonly [string, string])[] = [
  ['Controls', 'Controls'],
  ['Nodes', 'Nodes and layout'],
  ['Text', 'Text and fonts'],
  ['Drawing', 'Drawing'],
  ['Assets', 'Images and assets'],
  ['Core', 'Application, events and theming'],
  ['Platform', 'Platform, persistence, fetch and workers'],
  ['FuiCs.Worker', 'Worker projects (FuiCs.Worker)'],
  ['Generated', 'Enumerations shared with the runtime'],
  ['Hosting', 'Host entry points'],
  ['Abi', 'Runtime ABI'],
];

const PRIMITIVES: Readonly<Record<string, string>> = {
  'System.String': 'string', 'System.Int32': 'int', 'System.UInt32': 'uint', 'System.Int64': 'long', 'System.UInt64': 'ulong',
  'System.Single': 'float', 'System.Double': 'double', 'System.Boolean': 'bool', 'System.Byte': 'byte', 'System.Object': 'object',
};

async function sourceFiles(root: string): Promise<string[]> {
  const files: string[] = [];
  for (const entry of await fs.readdir(root, { withFileTypes: true })) {
    const full = path.join(root, entry.name);
    if (entry.isDirectory()) {
      if (entry.name !== 'bin' && entry.name !== 'obj') files.push(...await sourceFiles(full));
    } else if (entry.name.endsWith('.cs')) {
      files.push(full);
    }
  }
  return files;
}

/** Splits a base list or parameter list on its top-level commas. */
function splitTopLevel(text: string): string[] {
  const parts: string[] = [];
  let depth = 0;
  let current = '';
  for (const character of text) {
    if (character === '<' || character === '(' || character === '{') depth += 1;
    if (character === '>' || character === ')' || character === '}') depth -= 1;
    if (character === ',' && depth === 0) {
      parts.push(current.trim());
      current = '';
    } else {
      current += character;
    }
  }
  if (current.trim().length > 0) parts.push(current.trim());
  return parts;
}

/**
 * Adds the public types declared under `root`. The SDK's types are grouped by their top folder; a library's types
 * (`groupAs`) are grouped under the library.
 */
async function scanSources(root: string, types: Map<string, SourceType>, groupAs?: string): Promise<void> {
  const declaration = /^public\s+(?:(?:sealed|static|abstract|partial|readonly)\s+)*(class|record struct|record|struct|enum|interface)\s+(\w+)(<[^>]+>)?/;
  for (const file of await sourceFiles(root)) {
    const relative = path.relative(root, file).split(path.sep);
    const folder = groupAs ?? (relative.length > 1 ? relative[0] ?? '' : '');
    const text = await fs.readFile(file, 'utf8');
    const namespace = (/^namespace\s+FuiCs\.?([\w.]*);/m.exec(text)?.[1]) ?? '';
    const lines = text.split('\n');
    for (let index = 0; index < lines.length; index += 1) {
      const line = lines[index] ?? '';
      const match = declaration.exec(line);
      if (match === null) continue;
      const [, kind = '', name = '', generics = ''] = match;
      // The declaration runs to its opening brace; the base list follows the last top-level ':'.
      let header = line;
      for (let next = index + 1; !header.includes('{') && !header.trimEnd().endsWith(';') && next < lines.length; next += 1) {
        header += ` ${(lines[next] ?? '').trim()}`;
      }
      const afterName = header.slice(header.indexOf(name) + name.length + generics.length);
      const withoutParameters = afterName.startsWith('(') ? afterName.slice(matchingParen(afterName) + 1) : afterName;
      const colon = withoutParameters.indexOf(':');
      const brace = withoutParameters.indexOf('{');
      const baseText = colon >= 0 && (brace < 0 || colon < brace) ? withoutParameters.slice(colon + 1, brace < 0 ? undefined : brace) : '';
      const bases = splitTopLevel(baseText.replace(/\bwhere\b.*$/, '')).map((base) => base.replace(/\(.*$/, '').replace(/<.*$/, '').trim());
      const enumMembers: string[] = [];
      if (kind === 'enum') {
        for (let next = index + 1; next < lines.length && !(lines[next] ?? '').startsWith('}'); next += 1) {
          const member = /^\s+(\w+)\s*(=\s*[^,]+)?,?\s*$/.exec(lines[next] ?? '');
          if (member?.[1] !== undefined) enumMembers.push(member[1]);
        }
      }
      const arity = generics.length > 0 ? `\`${String(generics.split(',').length)}` : '';
      const key = `${namespace.length > 0 ? `${namespace}.` : ''}${name}${arity}`;
      types.set(key, { name: key, namespace, displayName: name + generics, kind, folder, bases, enumMembers });
    }
  }
}

/** The key of a base type named in `type`'s declaration: in its namespace, an enclosing one, or FuiCs. */
function resolveBase(type: SourceType | undefined, base: string, types: ReadonlyMap<string, SourceType>): string {
  const parts = type?.namespace.split('.').filter((part) => part.length > 0) ?? [];
  for (let length = parts.length; length >= 0; length -= 1) {
    const key = [...parts.slice(0, length), base].join('.');
    if (types.has(key)) return key;
  }
  return base;
}

function matchingParen(text: string): number {
  let depth = 0;
  for (let index = 0; index < text.length; index += 1) {
    if (text[index] === '(') depth += 1;
    if (text[index] === ')') {
      depth -= 1;
      if (depth === 0) return index;
    }
  }
  return text.length - 1;
}

function parseXml(xml: string): Map<string, XmlMember> {
  const members = new Map<string, XmlMember>();
  for (const match of xml.matchAll(/<member name="([^"]+)">([\s\S]*?)<\/member>/g)) {
    const [, id = '', body = ''] = match;
    members.set(id, { id, body });
  }
  return members;
}

/** The type key (`Name` or `Name`n`) and member part of an XML member id. */
function splitId(id: string): { readonly kind: string; readonly typeKey: string; readonly member: string } {
  const kind = id.charAt(0);
  const rest = id.slice(2).replace(/^FuiCs\./, '');
  if (kind === 'T') return { kind, typeKey: rest, member: '' };
  const paren = rest.indexOf('(');
  const head = paren >= 0 ? rest.slice(0, paren) : rest;
  const dot = head.lastIndexOf('.', head.endsWith('.#ctor') ? head.length - 7 : head.length);
  return { kind, typeKey: head.slice(0, dot), member: rest.slice(dot + 1) };
}

function simplifyType(type: string, generics: readonly string[]): string {
  let result = type.replace(/``(\d+)/g, (_, index: string) => generics[Number(index)] ?? 'T').replace(/`(\d+)/g, (_, index: string) => `T${index}`);
  result = result.replace(/System\.Nullable\{([^}]+)\}/g, '$1?');
  for (const [full, short] of Object.entries(PRIMITIVES)) result = result.split(full).join(short);
  result = result.replace(/\{/g, '<').replace(/\}/g, '>')
    .replace(/\b(System\.Collections\.Generic|System\.Threading\.Tasks|System\.Threading|System|FuiCs\.Workers\.Hosting|FuiCs\.Workers|FuiCs\.Hosting|FuiCs\.Testing|FuiCs)\./g, '');
  return result.replace(/@$/, '').replace(/,/g, ', ');
}

/** A member's signature; an extension method's omits its receiver, as callers write it. */
function signatureOf(kind: string, typeName: string, member: string, isExtension: boolean): string {
  const paren = member.indexOf('(');
  const name = (paren >= 0 ? member.slice(0, paren) : member).replace(/``\d+$/, '').replace('#ctor', typeName.replace(/<.*$/, ''));
  if (kind !== 'M') return name;
  const genericCount = /``(\d+)/.exec(member)?.[1];
  const generics = genericCount === undefined ? [] : Array.from({ length: Number(genericCount) }, (_, index) => (index === 0 ? 'T' : `T${String(index)}`));
  const parameters = paren >= 0 ? splitTopLevel(member.slice(paren + 1, -1)).map((parameter) => {
    const byRef = parameter.endsWith('@');
    return (byRef ? 'out ' : '') + simplifyType(parameter, generics);
  }).slice(isExtension ? 1 : 0) : [];
  return `${name}(${parameters.join(', ')})`;
}

function tagText(body: string, tag: string): string {
  return new RegExp(`<${tag}>([\\s\\S]*?)</${tag}>`).exec(body)?.[1] ?? '';
}

function toMarkdown(xmlText: string, types: ReadonlyMap<string, SourceType>): string {
  return xmlText
    .replace(/<see cref="([A-Z]):([^"]+)"\s*\/>/g, (_, kind: string, target: string) => {
      const { typeKey, member } = splitId(`${kind}:${target}`);
      const typeLabel = types.get(typeKey)?.displayName.replace(/<.*$/, '') ?? typeKey.replace(/`\d+$/, '').replace(/^.*\./, '');
      const label = kind === 'T' ? typeLabel : `${typeLabel}.${member.replace(/\(.*$/, '').replace(/``\d+$/, '')}`;
      const page = types.has(typeKey) ? `[\`${label}\`](${fileName(typeKey)})` : `\`${label}\``;
      return page;
    })
    .replace(/<see langword="([^"]+)"\s*\/>/g, '`$1`')
    .replace(/<(?:paramref|typeparamref) name="([^"]+)"\s*\/>/g, '`$1`')
    .replace(/<c>([\s\S]*?)<\/c>/g, '`$1`')
    .replace(/<\/?para>/g, '\n\n')
    .replace(/<[^>]+>/g, '')
    .replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&amp;/g, '&').replace(/&quot;/g, '"')
    .split('\n').map((line) => line.trim()).join(' ').replace(/ {2,}/g, ' ').trim();
}

function fileName(typeKey: string): string {
  return `${typeKey.replace(/`(\d+)$/, '-$1')}.md`;
}

/** The docs of a member marked <inheritdoc/>: the same member on a base type or interface, searched breadth first. */
function inheritedBody(member: XmlMember, types: ReadonlyMap<string, SourceType>, members: ReadonlyMap<string, XmlMember>): string {
  const { kind, typeKey, member: memberPart } = splitId(member.id);
  const basesOf = (key: string): string[] => (types.get(key)?.bases ?? []).map((base) => resolveBase(types.get(key), base, types));
  const queue = basesOf(typeKey);
  const seen = new Set<string>();
  while (queue.length > 0) {
    const base = queue.shift() ?? '';
    if (seen.has(base)) continue;
    seen.add(base);
    const candidate = members.get(`${kind}:FuiCs.${base}.${memberPart}`) ?? members.get(`${kind}:${base}.${memberPart}`);
    if (candidate !== undefined && !candidate.body.includes('<inheritdoc')) return candidate.body;
    queue.push(...basesOf(base));
    if (base === 'Node' || base === 'IDisposable') continue;
  }
  if (memberPart === 'Dispose') return '<summary>Releases the object and its native resources.</summary>';
  return '';
}

function render(types: ReadonlyMap<string, SourceType>, members: ReadonlyMap<string, XmlMember>): Map<string, string> {
  const pages = new Map<string, string>();
  const entries = new Map<string, Entry[]>();
  for (const member of members.values()) {
    const { kind, typeKey, member: memberPart } = splitId(member.id);
    if (kind === 'T' || !types.has(typeKey)) continue;
    // A C# 14 extension member's method inherits the docs written on its extension block's member.
    const cref = /<inheritdoc cref="([^"]+)"\s*\/>/.exec(member.body)?.[1];
    const isExtension = cref?.includes('&lt;G&gt;$') ?? false;
    const body = cref !== undefined ? members.get(cref)?.body ?? ''
      : member.body.includes('<inheritdoc') ? inheritedBody(member, types, members) : member.body;
    const text = toMarkdown(tagText(body, 'summary'), types);
    const list = entries.get(typeKey) ?? [];
    list.push({ kind, signature: signatureOf(kind, types.get(typeKey)?.displayName ?? typeKey, memberPart, isExtension), text });
    entries.set(typeKey, list);
  }
  const index: string[] = ['# FUI-CS API reference', '', 'One page per public type, generated from the SDK\'s XML documentation by `npm run generate:docs`.', ''];
  for (const [folder, title] of AREAS) {
    const inArea = [...types.values()].filter((type) => type.folder === folder).sort((a, b) => a.displayName.localeCompare(b.displayName));
    if (inArea.length === 0) continue;
    index.push(`## ${title}`, '', '| Type | Kind | Summary |', '|---|---|---|');
    for (const type of inArea) {
      const summary = toMarkdown(tagText(members.get(`T:FuiCs.${type.name}`)?.body ?? '', 'summary'), types);
      index.push(`| [\`${type.displayName}\`](${fileName(type.name)}) | ${type.kind} | ${firstSentence(summary)} |`);
      pages.set(fileName(type.name), renderType(type, summary, entries.get(type.name) ?? [], types));
    }
    index.push('');
  }
  pages.set('README.md', `${index.join('\n').trimEnd()}\n`);
  return pages;
}

function firstSentence(text: string): string {
  const end = text.search(/\.(\s|$)/);
  return (end >= 0 ? text.slice(0, end + 1) : text).replace(/\|/g, '\\|');
}

function renderType(type: SourceType, summary: string, entries: readonly Entry[], types: ReadonlyMap<string, SourceType>): string {
  const baseLinks = type.bases.map((base) => {
    const key = resolveBase(type, base, types);
    return types.has(key) ? `[\`${base}\`](${fileName(key)})` : `\`${base}\``;
  });
  const namespace = type.namespace.length > 0 ? `FuiCs.${type.namespace}` : 'FuiCs';
  const lines = [`# ${type.displayName}`, '', `${type.kind} in \`${namespace}\`${baseLinks.length > 0 ? `: ${baseLinks.join(', ')}` : ''}`, ''];
  if (summary.length > 0) lines.push(summary, '');
  const extensions = types.get(`${type.name}Extensions`);
  if (extensions !== undefined) lines.push(`Fluent members: [\`${extensions.displayName}\`](${fileName(extensions.name)}).`, '');
  if (type.enumMembers.length > 0) {
    lines.push('## Values', '');
    for (const value of type.enumMembers) {
      const doc = entries.find((entry) => entry.kind === 'F' && entry.signature === value)?.text ?? '';
      lines.push(`- \`${value}\`${doc.length > 0 ? `: ${doc}` : ''}`);
    }
    lines.push('');
  }
  const sections: readonly (readonly [string, (entry: Entry) => boolean])[] = [
    ['Constructors', (entry) => entry.kind === 'M' && entry.signature.startsWith(`${type.displayName.replace(/<.*$/, '')}(`)],
    ['Properties', (entry) => entry.kind === 'P'],
    ['Methods', (entry) => entry.kind === 'M' && !entry.signature.startsWith(`${type.displayName.replace(/<.*$/, '')}(`)],
    ['Events', (entry) => entry.kind === 'E'],
    ['Fields', (entry) => entry.kind === 'F' && type.enumMembers.length === 0],
  ];
  for (const [title, include] of sections) {
    const chosen = entries.filter(include).sort((a, b) => a.signature.localeCompare(b.signature));
    if (chosen.length === 0) continue;
    lines.push(`## ${title}`, '');
    for (const entry of chosen) lines.push(`- \`${entry.signature}\`${entry.text.length > 0 ? `: ${entry.text}` : ''}`);
    lines.push('');
  }
  return `${lines.join('\n').trimEnd()}\n`;
}

async function main(): Promise<void> {
  const commandLine = parseCommandLine(process.argv.slice(2), USAGE, 5);
  const [outputDir = '', sdkXml = '', sdkSources = '', workerXml = '', workerSources = ''] = commandLine.positional;
  const types = new Map<string, SourceType>();
  await scanSources(sdkSources, types);
  await scanSources(workerSources, types, 'FuiCs.Worker');
  const members = new Map([...parseXml(await fs.readFile(sdkXml, 'utf8')), ...parseXml(await fs.readFile(workerXml, 'utf8'))]);
  const pages = render(types, members);
  let stale = false;
  const existing = new Set((await fs.readdir(outputDir).catch(() => [])).filter((file) => file.endsWith('.md')));
  for (const [file, content] of pages) {
    const target = path.join(outputDir, file);
    existing.delete(file);
    if (commandLine.check) {
      if (await fs.readFile(target, 'utf8').catch(() => null) !== content) stale = true;
      continue;
    }
    await fs.mkdir(outputDir, { recursive: true });
    await fs.writeFile(target, content, 'utf8');
  }
  for (const leftover of existing) {
    if (commandLine.check) stale = true;
    else await fs.rm(path.join(outputDir, leftover));
  }
  if (stale) {
    console.error(`${path.relative(process.cwd(), outputDir)} is out of date; run npm run generate:docs.`);
    process.exitCode = 1;
  }
}

await main();
