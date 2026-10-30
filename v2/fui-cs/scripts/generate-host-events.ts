// Generates an application's C# host events, and the [JSExport] functions the harness raises them through, from
// its defineHostEvents definitions.

import path from 'node:path';

import { renderCSharpHostEvents } from '../../hostgen/src/csharp-host-events.ts';
import { parseCommandLine, requiredOption, writeOrCheck } from './hostgen-cli.ts';

const USAGE = 'Usage: generate-host-events <module-path> <export-name> <csharp-output> --namespace <namespace> [--check]';

const commandLine = parseCommandLine(process.argv.slice(2), USAGE, 3);
const [modulePath, exportName, csharpOutput] = commandLine.positional as [string, string, string];
const csharp = await renderCSharpHostEvents(path.resolve(modulePath), exportName, {
  namespace: requiredOption(commandLine, 'namespace', USAGE),
});
if (!await writeOrCheck(path.resolve(csharpOutput), csharp, commandLine.check)) process.exitCode = 1;
