import {env} from "process";
import axios  from "axios";
const {ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS} = env;
const target = ASPNETCORE_URLS ? ASPNETCORE_URLS.split(';')[1] : 'http://localhost:5005';
console.info({target, ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS})

 export const instance = axios.create({
    baseURL: `${target}/schema/`,
    timeout: 1000,
    headers: {'X-Custom-Header': 'foobar'},
    httpsAgent: {
        rejectUnauthorized: false,
    }

});

export default instance;
