/**
 * The JSImport module a fui-cs worker project runs inside its Web Worker. It sends the running job's progress to the
 * page as worker-client notifications and runs the job's fetches; the job's result and failure travel as the
 * `__fui_worker_run` export's own result, and the page cancels the job through the `__fui_worker_cancel` export.
 */
type Notify = (operation: string, values: readonly unknown[]) => void;

const fetches = new Map<number, AbortController>();

/** Headers arrive as alternating name and value lines; HTTP forbids line breaks in either. */
function parseHeaders(encoded: string): [string, string][] {
  const lines = encoded.split('\n');
  const headers: [string, string][] = [];
  for (let index = 0; index + 1 < lines.length; index += 2) {
    headers.push([lines[index] ?? '', lines[index + 1] ?? '']);
  }
  return headers;
}

/** Sends a worker job's request and answers "ok\nstatus\nstatusText\nurl"; a network failure rejects. */
async function sendFetch(requestId: number, method: string, url: string, headers: string, body: Uint8Array): Promise<string> {
  const controller = new AbortController();
  fetches.set(requestId, controller);
  try {
    const init: RequestInit = { method, headers: parseHeaders(headers), signal: controller.signal };
    if (body.length > 0) {
      // A copy over a plain ArrayBuffer, as fetch requires.
      init.body = new Uint8Array(body);
    }
    const response = await fetch(url, init);
    return [response.ok ? '1' : '0', String(response.status), response.statusText, response.url].join('\n');
  } finally {
    fetches.delete(requestId);
  }
}

export function createWorkerImports(notify: Notify): Record<string, (...args: never[]) => unknown> {
  return {
    notify(operation: string, text: string): void {
      notify(operation, [text]);
    },
    fetch: sendFetch,
    abortFetch(requestId: number): void {
      fetches.get(requestId)?.abort();
    },
  };
}
