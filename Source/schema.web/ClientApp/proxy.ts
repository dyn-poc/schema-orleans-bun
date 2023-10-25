import {createProxyMiddleware, RequestHandler} from 'http-proxy-middleware';
import {env} from 'process'

//declare env type


const {ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS} = env;
const target = ASPNETCORE_HTTPS_PORT ? `https://localhost:${ASPNETCORE_HTTPS_PORT}` :
ASPNETCORE_URLS ? ASPNETCORE_URLS.split(';')[0] : 'http://localhost:26276';

const context =  [
  "/schema",
];

export const Proxy = createProxyMiddleware(context, {
  target: target,
  changeOrigin: true,
  secure: false,
  headers: {
    Connection: 'Keep-Alive'
  }
});


export default Proxy;
