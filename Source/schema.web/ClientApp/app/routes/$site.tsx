import EventEmitter from 'events'

import React from 'react'
import {json, LoaderFunctionArgs} from "@remix-run/node";
import schema from "~/services/schema";
import {Link, NavLink, Outlet, useLoaderData, useMatch} from "@remix-run/react";
import { JSONPath } from '@astronautlabs/jsonpath';
import clsx from "clsx";
import * as path from "path";
import Json from '~/editor';

const mock = {
 "$schema": "http://json-schema.org/draft-04/schema#",
  "description": "A person - mock data",
  "$id": "https://example.com/person.schema.json",
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
        const response = await schema(`${site}`);
        console.log(`GET ${site}:`, {
            status_code: response.status,
            status_text: response.statusText,
            headers: JSON.stringify(response.headers, null, 2),
            data: response.data
        });
        const {data } = response.data? response : {data: mock};

        return json( {site, json: data || mock})

    }catch (e) {
        console.error(e);
        return json( mock)
    }
}



export default function SchemaViewer() {

    const {json, site} = useLoaderData();


    console.log("json", {json, site});
  //find all the $ref in the json
  const refs = JSONPath.query(json || {}, "$..['$ref']").map(
    (ref: string) => {return {
        href:  path.isAbsolute(ref) ? `absolute` : ref,
        ref: ref,
        absolute: ref.startsWith("http"),
        def: ref.startsWith( "#/definitions/") || ref.startsWith( "#/components/schemas/") || ref.startsWith( "#/$defs/")

    }}
  );



  return (
    <>
        <div
            className="fixed overflow-hidden w-full flex"
            style={{ height: `calc(100%)` }}
        >


          <NavLink
            to='.'
            relative={"route"}
            style={({ isActive, isPending }) => {
              return {
                fontWeight: isActive ? "bold" : ""};
            }}
            state={{ ref: site, absolute: false, href: site }}
          >
            <h2>{site}</h2>
          </NavLink>
              <aside
                  className={clsx(
                      "px-4 py-6 bg-gray-300 relative h-full overflow-y-auto max-w-max"
                  )}
              >
                  <nav>
                      <ul className="space-y-2">
                          {refs.filter(r=> !r.absolute).map(({href, ref, absolute}) => (
                              <li key={ref}>
                                  <NavLink
                                      to={href}
                                      relative={"route"}
                                      style={({ isActive, isPending }) => {
                                          return {
                                              fontWeight: isActive ? "bold" : "",
                                              color: isPending ? "red" : "black",
                                          };
                                      }}
                                      state={{ ref: ref, absolute: absolute, href: href }}
                                   >
                                      {ref}
                                  </NavLink>
                              </li>
                          ))}
                      </ul>
                  </nav>
              </aside>
          <Outlet context={{site  }} />

          <Json  src={json}/>

      </div>
    </>
  )
}
