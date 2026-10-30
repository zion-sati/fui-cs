// The host services the demo shell offers its routes. Routes call them through bindings their projects generate
// when they build (FuiCsHostServices).

import { defineHostServices, hostService } from '@effindomv2/runtime/managed-harness';

import { formatDemoShellAccentColor, readDemoShellState } from './shell-state.ts';

export const demoHostServices = defineHostServices({
  demoShell: {
    wallClockSinceEpochMs: hostService({
      args: [] as const,
      returns: 'f64',
      implementation: () => Date.now(),
    }),
    clockTickSeconds: hostService({
      args: [] as const,
      returns: 'i32',
      implementation: () => readDemoShellState().tick,
    }),
    accentColorHex: hostService({
      args: [] as const,
      returns: 'string',
      implementation: () => formatDemoShellAccentColor(),
    }),
    isDarkMode: hostService({
      args: [] as const,
      returns: 'bool',
      implementation: () => readDemoShellState().darkMode,
    }),
  },
});
