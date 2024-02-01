import EventEmitter from 'events'

import React from 'react'
import {json, LoaderFunctionArgs} from "@remix-run/node";
import {Link, NavLink, Outlet, useLoaderData, useMatch, useParams} from "@remix-run/react";
import { JSONPath } from '@astronautlabs/jsonpath';
import clsx from "clsx";
import * as path from "path";
import Json from '~/editor';

import {loader as schemaLoader} from './$site.schema.$';

export async function loader(args: LoaderFunctionArgs) {
  return schemaLoader(args);
}




export default function SchemaViewer() {
    const {site} = useParams();

    const json = useLoaderData();


   //find all the $ref in the json
  const refs = JSONPath.query(json || {}, "$..['$ref']").map(
    (ref: string) => {return {
        href:  path.isAbsolute(ref) ? `absolute` : ref,
        ref: ref,
        absolute: ref.startsWith("http"),
        def: ref.startsWith( "#/definitions/") || ref.startsWith( "#/components/schemas/") || ref.startsWith( "#/$defs/")

    }}
  ).filter(({def})=> !def)
      .reduce((acc, {href, ref, absolute}) => {
            if(!acc.find(({href: h})=> h === href)){
                acc.push({href, ref, absolute})
            }
            return acc;
        }, [] as {href: string, ref: string, absolute: boolean}[]);
    console.log("refs", {refs});



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

                      <ul className="space-y-2 nav">
                          <li className={"nav-item"}>
                              <NavLink
                                  to="bundled"
                                  relative={"route"}
                                  className={"nav-link"}
                                  style={({ isActive, isPending }) => {
                                      return {
                                          fontWeight: isActive ? "bold" : "",
                                          color: isPending ? "red" : "black",
                                      };
                                  }}
                                  state={{ ref: site, absolute: false, href: site }}
                              >
                                  bundled
                              </NavLink>
                          </li>
                        <li className={"nav-item"} key={"auth"}>
                          <NavLink
                            to="auth"
                            relative={"route"}
                            className={"nav-link"}
                            style={({ isActive, isPending }) => {
                              return {
                                fontWeight: isActive ? "bold" : "",
                                color: isPending ? "red" : "black",
                              };
                            }}
                            state={{ ref: site, absolute: false, href: site }}
                          >
                            auth
                          </NavLink>
                        </li>
                        <li className={"nav-item"} key={"guest"}>
                          <NavLink
                            to="guest"
                            relative={"route"}
                            className={"nav-link"}
                            style={({ isActive, isPending }) => {
                              return {
                                fontWeight: isActive ? "bold" : "",
                                color: isPending ? "red" : "black",
                              };
                            }}
                            state={{ ref: site, absolute: false, href: site }}
                          >
                            guest
                          </NavLink>
                        </li>
                        <li className={"nav-item"} key={"guest(bundled)"}>
                          <NavLink
                            to="guest(bundled)"
                            relative={"route"}
                            className={"nav-link"}
                            style={({ isActive, isPending }) => {
                              return {
                                fontWeight: isActive ? "bold" : "",
                                color: isPending ? "red" : "black",
                              };
                            }}
                            state={{ ref: site, absolute: false, href: site }}
                          >
                            guest(bundled)
                          </NavLink>
                        </li>
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

      </div>
    </>
  )
}
