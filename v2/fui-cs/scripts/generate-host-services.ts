// Generates an application's C# host-service bindings and the JSImport module they call from its
// defineHostServices definitions. The application declares the bundled module under --js-module.

import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { renderCSharpHostServices } from '../../hostgen/src/csharp-host-services.ts';
import { parseCommandLine, requiredOption, writeOrCheck } from './hostgen-cli.ts';

const USAGE = 'Usage: generate-host-services <module-path> <export-name> <csharp-output> <module-output> '
  + '--namespace <namespace> [--js-module <name>] [--target page|worker] [--check]';

/** A file of the package's browser runtime, found above this script whether it runs from scripts/ or bundled. */
function findRuntime(file: string): string {
  for (let dir = path.dirname(fileURLToPath(import.meta.url)); ; dir = path.dirname(dir)) {
    const candidate = path.join(dir, 'browser', file);
    if (existsSync(candidate)) return candidate;
    if (path.dirname(dir) === dir) throw new Error(`Cannot find browser/${file} above the generator.`);
  }
}

/** An import path from the generated module's folder to `target`. */
function importPath(fromFolder: string, target: string): string {
  const relative = path.relative(fromFolder, target).split(path.sep).join('/');
  return relative.startsWith('.') ? relative : `./${relative}`;
}

const commandLine = parseCommandLine(process.argv.slice(2), USAGE, 4);
const [moduleArg, exportName, csharpArg, moduleOutputArg] = commandLine.positional as [string, string, string, string];
const moduleOutput = path.resolve(moduleOutputArg);
const target = commandLine.options.get('target') ?? 'page';
if (target !== 'page' && target !== 'worker') throw new Error(`--target must be page or worker.\n${USAGE}`);
const worker = target === 'worker';
const output = await renderCSharpHostServices(path.resolve(moduleArg), exportName, {
  namespace: requiredOption(commandLine, 'namespace', USAGE),
  jsModuleName: commandLine.options.get('js-module') ?? (worker ? 'fui_worker_host_services' : 'fui_app_host_services'),
  runtimeImport: importPath(path.dirname(moduleOutput), findRuntime(worker ? 'worker/host-services.ts' : 'host-services.ts')),
  ...(worker ? { workerDefinitionsImport: importPath(path.dirname(moduleOutput), path.resolve(moduleArg)) } : {}),
});
const current = [
  await writeOrCheck(path.resolve(csharpArg), output.csharp, commandLine.check),
  await writeOrCheck(moduleOutput, output.module, commandLine.check),
];
if (current.includes(false)) process.exitCode = 1;
