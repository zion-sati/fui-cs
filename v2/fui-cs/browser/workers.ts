// Runs fui-cs workers: each Worker the C# app starts is a published fui-cs worker project, a NetWasm JSExport worker
// that this module starts through its generated client. It stands in for the managed harness's worker imports,
// which load raw wasm worker modules of their own shape.

import type { AppMemory } from './app-memory.ts';

/** What the app receives, through its `__fui_on_worker_message` export. */
const enum WorkerMessageKind {
  Progress = 0,
  Complete = 1,
  Error = 2,
}

interface WorkerNotification {
  readonly operation: string;
  readonly arguments: readonly unknown[];
}

interface WorkerClient {
  readonly [name: string]: unknown;
  terminate(): void;
}

interface WorkerClientModule {
  createWorker(options: { onNotification(notification: WorkerNotification): void }): Promise<WorkerClient>;
}

type RunExport = (entryName: string, input: string) => Promise<string>;
type CancelExport = () => Promise<void>;

interface RunningWorker {
  client: WorkerClient | null;
  cancelled: boolean;
}

export type DeliverWorkerMessage = (workerId: number, kind: number, text: string) => void;

function describeError(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }
  if (typeof error === 'object' && error !== null && typeof (error as { message?: unknown }).message === 'string') {
    return (error as { message: string }).message;
  }
  return String(error);
}

/**
 * The worker client module for `path`: the module itself when `path` names one, otherwise the client in a
 * published worker folder (`browser/workers/Name` holds `Name.worker-client.mjs`), relative to the page.
 */
export function workerClientUrl(path: string, base: string): string {
  if (path.endsWith('.mjs')) {
    return new URL(path, base).href;
  }
  const folder = path.endsWith('/') ? path : `${path}/`;
  const name = folder.slice(0, -1).split('/').pop() ?? '';
  return new URL(`${folder}${name}.worker-client.mjs`, base).href;
}

/** Asks the worker's running job to stop; the job sees it when it next yields, and reports how far it got. */
function cancelJob(client: WorkerClient): void {
  const cancel = client.__fui_worker_cancel;
  if (typeof cancel === 'function') {
    // A worker that has already finished rejects the call; the page has nothing more to do with it.
    void (cancel as CancelExport)().catch(() => undefined);
  }
}

/** Replaces the harness's `fui_worker_start_string` and `fui_worker_cancel` imports with NetWasm workers. */
export function installNetWasmWorkers(
  imports: WebAssembly.Imports,
  memory: AppMemory,
  deliver: DeliverWorkerMessage,
): void {
  const running = new Map<number, RunningWorker>();

  function finish(workerId: number, kind: WorkerMessageKind, text: string): void {
    const worker = running.get(workerId);
    if (worker === undefined) {
      return;
    }
    running.delete(workerId);
    worker.client?.terminate();
    deliver(workerId, kind, text);
  }

  function handleNotification(workerId: number, notification: WorkerNotification): void {
    const worker = running.get(workerId);
    const value = notification.arguments[0];
    if (worker === undefined || worker.cancelled || notification.operation !== 'progress') {
      return;
    }
    deliver(workerId, WorkerMessageKind.Progress, typeof value === 'string' ? value : '');
  }

  async function start(workerId: number, path: string, entryName: string, input: string): Promise<void> {
    const worker: RunningWorker = { client: null, cancelled: false };
    running.set(workerId, worker);
    try {
      const module = await import(/* @vite-ignore */ workerClientUrl(path, document.baseURI)) as WorkerClientModule;
      const client = await module.createWorker({
        onNotification: (notification) => {
          handleNotification(workerId, notification);
        },
      });
      if (running.get(workerId) !== worker) {
        client.terminate();
        return;
      }
      worker.client = client;
      const run = client.__fui_worker_run;
      if (typeof run !== 'function') {
        throw new Error(`${path} is not a fui-cs worker: it has no __fui_worker_run export.`);
      }
      // The worker handles its calls in order, so a cancellation sent after the run reaches the running job.
      const result = (run as RunExport)(entryName, input);
      if (worker.cancelled) {
        cancelJob(client);
      }
      finish(workerId, WorkerMessageKind.Complete, await result);
    } catch (error: unknown) {
      finish(workerId, WorkerMessageKind.Error, describeError(error));
    }
  }

  // The harness always provides fui_host; its worker imports are replaced in place.
  const host = imports.fui_host as Record<string, unknown>;
  host.fui_worker_start_string = (
    workerId: number,
    pathPtr: number,
    pathLen: number,
    entryPtr: number,
    entryLen: number,
    inputPtr: number,
    inputLen: number,
  ): void => {
    if (running.has(workerId)) {
      deliver(workerId, WorkerMessageKind.Error, 'Worker already started.');
      return;
    }
    void start(workerId, memory.readUtf8(pathPtr, pathLen), memory.readUtf8(entryPtr, entryLen), memory.readUtf8(inputPtr, inputLen));
  };
  host.fui_worker_cancel = (workerId: number): void => {
    const worker = running.get(workerId);
    if (worker === undefined || worker.cancelled) {
      return;
    }
    worker.cancelled = true;
    if (worker.client !== null) {
      cancelJob(worker.client);
    }
  };
}
