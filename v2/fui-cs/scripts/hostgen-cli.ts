// Command-line handling shared by the host-service and host-event generators: positional arguments, options,
// and --check, which fails when a generated file is out of date instead of writing it.

import { promises as fs } from 'node:fs';
import path from 'node:path';

export interface HostgenCommandLine {
  readonly positional: readonly string[];
  readonly options: ReadonlyMap<string, string>;
  readonly check: boolean;
}

export function parseCommandLine(args: readonly string[], usage: string, positionalCount: number): HostgenCommandLine {
  const positional: string[] = [];
  const options = new Map<string, string>();
  let check = false;
  for (let index = 0; index < args.length; index += 1) {
    const arg = args[index] ?? '';
    if (arg === '--check') {
      check = true;
    } else if (arg.startsWith('--')) {
      if (index + 1 >= args.length) throw new Error(`${arg} needs a value.\n${usage}`);
      options.set(arg.slice(2), args[index + 1] ?? '');
      index += 1;
    } else {
      positional.push(arg);
    }
  }
  if (positional.length !== positionalCount) throw new Error(usage);
  return { positional, options, check };
}

export function requiredOption(commandLine: HostgenCommandLine, name: string, usage: string): string {
  const value = commandLine.options.get(name);
  if (value === undefined) throw new Error(`--${name} is required.\n${usage}`);
  return value;
}

/** Writes `content` to `file`, or with --check reports whether it already matches; returns false when stale. */
export async function writeOrCheck(file: string, content: string, check: boolean): Promise<boolean> {
  const existing = await fs.readFile(file, 'utf8').catch(() => null);
  // An unchanged file keeps its timestamp, so incremental builds do not recompile what depends on it.
  if (existing === content) return true;
  if (check) {
    console.error(`${path.relative(process.cwd(), file)} is out of date; build to regenerate it.`);
    return false;
  }
  await fs.mkdir(path.dirname(file), { recursive: true });
  await fs.writeFile(file, content, 'utf8');
  return true;
}
