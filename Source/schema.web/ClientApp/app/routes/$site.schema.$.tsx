import {ActionFunctionArgs, json, LoaderFunctionArgs} from "@remix-run/node";
import axios from "axios";
import {env} from "process";
import Json from "~/editor";
import {useLoaderData} from "@remix-run/react";
import React from "react";
const {ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS} = env;
const target = ASPNETCORE_URLS ? ASPNETCORE_URLS.split(';')[1] : 'http://localhost:5005';
console.info({target, ASPNETCORE_HTTPS_PORT, ASPNETCORE_URLS})

 const instance = axios.create({
  baseURL: `${target}/schema/`,
  timeout: 1000,
  headers: {'X-Custom-Header': 'foobar'},
  httpsAgent: {
    rejectUnauthorized: false,
  }

});
export async function loader({
                               params:{site, ["*"]: slug},

                             }: LoaderFunctionArgs) {
  try{

    const url=  slug? `${site}/${slug}` : `${site}`;
    const response =  await instance.get(url);
    console.log(`GET ${url}` , {
      status_code: response.status,
      status_text: response.statusText,
      id: response.data.$id
    });

    const {data } = response;
    return json( data  || {})

  }catch (e) {
    console.error(JSON.stringify(e, null, 2));
    return json({error:e})
  }
}


export const action = async ({
                               params:{site, ["*"]: slug},
                               request,
                             }: ActionFunctionArgs) => {

  const formData = await request.formData();

  const response = await instance.post(`${site}/${slug}`, {
    method: "POST",
    data: formData.get("input"),
    headers: {
      "Content-Type": "application/json",
    },
  });

  const {data} = response;
  console.log("response", {
    status_code: response.status,
    status_text: response.statusText
   });
  return json(data);

};


export default function SchemaViewer() {

  return (
    <>
      <div className="m-5 p-2" >

        <Json   src={useLoaderData<typeof loader>()}/>

      </div>
    </>
  )
}
