import {env} from "process";
import axios  from "axios";
import https from "https";
const {ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS} = env;
const target = ASPNETCORE_URLS ? ASPNETCORE_URLS.split(';')[0] : 'http://localhost:5005';
console.info({target, ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS})

const agent = new https.Agent({
  rejectUnauthorized: false,
})
export const instance = axios.create({
    baseURL: `${target}/schema/`,
    timeout: 1000,
    headers: {'X-Custom-Header': 'foobar'},
    httpsAgent: agent

});

export default instance;
