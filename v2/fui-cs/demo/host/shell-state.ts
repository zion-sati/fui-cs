// The demo shell's own state, which routes read through host services and follow through host events: a clock that
// ticks once a second, an accent color and dark mode, which follows the browser's color scheme. Tests drive it
// through window.__fuiDemoShell.

type Listener<T> = (value: T) => void;

interface DemoShellState {
  tick: number;
  accentColor: number;
  darkMode: boolean;
}

export interface DemoShellControls {
  /** Sets the clock and stops it ticking on its own. */
  setTick(tick: number): void;
  setAccentColor(rgba: number): void;
  setDarkMode(darkMode: boolean): void;
}

declare global {
  interface Window {
    __fuiDemoShell?: DemoShellControls;
  }
}

const state: DemoShellState = { tick: 0, accentColor: 0x2563ebff, darkMode: false };
const tickListeners = new Set<Listener<number>>();
const accentColorListeners = new Set<Listener<number>>();
const darkModeListeners = new Set<Listener<boolean>>();
let clock: ReturnType<typeof setInterval> | undefined;

function listen<T>(listeners: Set<Listener<T>>, listener: Listener<T>): () => void {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
}

function emit<T>(listeners: ReadonlySet<Listener<T>>, value: T): void {
  for (const listener of listeners) listener(value);
}

export function readDemoShellState(): Readonly<DemoShellState> {
  return state;
}

export function subscribeDemoShellTick(listener: Listener<number>): () => void {
  return listen(tickListeners, listener);
}

export function subscribeDemoShellAccentColor(listener: Listener<number>): () => void {
  return listen(accentColorListeners, listener);
}

export function subscribeDemoShellDarkMode(listener: Listener<boolean>): () => void {
  return listen(darkModeListeners, listener);
}

/** The accent color as #rrggbb. */
export function formatDemoShellAccentColor(): string {
  return `#${(state.accentColor >>> 8).toString(16).padStart(6, '0')}`;
}

function setTick(tick: number): void {
  state.tick = tick | 0;
  emit(tickListeners, state.tick);
}

function setDarkMode(darkMode: boolean): void {
  state.darkMode = darkMode;
  emit(darkModeListeners, darkMode);
}

/** Starts the shell clock, follows the browser's color scheme, and installs the controls tests use. */
export function startDemoShell(): void {
  clock ??= setInterval(() => { setTick(state.tick + 1); }, 1_000);
  const darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
  state.darkMode = darkModeQuery.matches;
  darkModeQuery.addEventListener('change', (event) => { setDarkMode(event.matches); });
  window.__fuiDemoShell = {
    setTick(tick): void {
      clearInterval(clock);
      setTick(tick);
    },
    setAccentColor(rgba): void {
      state.accentColor = rgba >>> 0;
      emit(accentColorListeners, state.accentColor);
    },
    setDarkMode,
  };
}
