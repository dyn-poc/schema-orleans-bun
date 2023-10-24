import EventEmitter from 'events'

import React from 'react'
import {json, LoaderFunctionArgs} from "@remix-run/node";
import schema from "~/services/schema";
import {useLoaderData} from "@remix-run/react";
import Json from "~/components/Json";

const mock = {
  "title": "Person",
  "type": "object",
  "properties": {
    "firstName": {
      "type": "string",
      "description": "The person's first name."
    }, "lastName": {"type": "string", "description": "The person's last name."},
  }
};
export async function loader({
                               params:{site}
                             }: LoaderFunctionArgs) {
  try{
    const response = await schema(site as string);

    return json({site, schema: response.data})

  }catch (e) {
    console.error(e);
    return json({site, schema: {error:e, ...mock}})
  }
}


export default function SchemaViewer() {
  const {schema, site} = useLoaderData<{ schema: any, site: string }>();
  return (
    <>
      <div className="m-5 p-2" >
        <h2>{site}</h2>
        <Json src={schema}/>

      </div>
    </>
  )
}
