import type {ServerBuild} from "@remix-run/server-runtime";
import {createRequestHandler, logDevReady} from "@remix-run/server-runtime";
import * as build from "./build/index";

if (Bun.env.NODE_ENV === "development")
  logDevReady(build as unknown as ServerBuild);


export const server = Bun.serve({
  port: Bun.env.PORT || 3003,
  development: true,
  hostname: Bun.env.HOSTNAME || "localhost",

  async fetch(request) {
    console.log("fetch", request.url);

    return tryPing(request) ||
    tryPublicFolder(request) ||
      createRequestHandler(
        build as unknown as ServerBuild,
        "development",
      )(request);
  }
});

Bun.serve({
  port:  44414,
  development: true,
  hostname: server.hostname,
  async fetch(request) {
    console.log("fetch", request.url);
    return new Response("<html lang='en'>hello</html>");

  }});

function tryPing(request: Request) {
  const {pathname, hostname} = new URL(request.url);
  if (hostname != server.hostname || pathname !== "/ping" ) return false;
  return new Response("pong");
}

function tryPublicFolder(request: Request) {
  const {pathname, hostname} = new URL(request.url);
  if (hostname !== server.hostname) return false;
  try {
    const resolvedpath = Bun.resolveSync(`.${pathname}`, "./public");
    console.debug("resolved", resolvedpath)
    return new Response(Bun.file(resolvedpath));

  } catch (e) {
    console.log("not found", pathname, e);
    return false;
  }

}


console.log(`service is ready at port http://${server.hostname}:${server.port}`);

//export default server;
