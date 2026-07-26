import { cp, mkdir, rm } from "node:fs/promises";
import path from "node:path";

const source = path.resolve("node_modules/@effindomv2/runtime/dist");
const destination = path.resolve("public/v2/browser-bridge");

await rm(destination, { force: true, recursive: true });
await mkdir(destination, { recursive: true });
await cp(source, destination, { recursive: true });
console.log(`Staged the published EffinDOM runtime in ${destination}`);
