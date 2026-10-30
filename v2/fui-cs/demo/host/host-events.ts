// The host events the demo shell raises in its routes. Routes handle them through C# events their projects generate
// when they build (FuiCsHostEvents).

import { defineHostEvents, hostEvent } from '@effindomv2/runtime/managed-harness';

import {
  readDemoShellState,
  subscribeDemoShellAccentColor,
  subscribeDemoShellDarkMode,
  subscribeDemoShellTick,
} from './shell-state.ts';

export const demoHostEvents = defineHostEvents({
  demoShell: {
    clockTickChanged: hostEvent({
      args: ['i32'] as const,
      subscribe(emit) {
        emit(readDemoShellState().tick);
        return subscribeDemoShellTick((tick) => { emit(tick); });
      },
    }),
    accentColorChanged: hostEvent({
      args: ['u32'] as const,
      subscribe(emit) {
        emit(readDemoShellState().accentColor);
        return subscribeDemoShellAccentColor((rgba) => { emit(rgba); });
      },
    }),
    darkModeChanged: hostEvent({
      args: ['bool'] as const,
      subscribe(emit) {
        emit(readDemoShellState().darkMode);
        return subscribeDemoShellDarkMode((darkMode) => { emit(darkMode); });
      },
    }),
  },
});
