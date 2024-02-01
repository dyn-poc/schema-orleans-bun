import {ActionFunctionArgs, json, LoaderFunctionArgs, MetaFunction} from "@remix-run/node";
 import {Form, Outlet, useActionData, useLoaderData, useLocation, useOutletContext} from "@remix-run/react";
import Json from "~/editor";
import React from "react";

import {loader as schemaLoader} from './$site.schema.$';
import {action as schemaAction} from './$site.schema.$';

export async function loader({
                                 params:{site, schema: slug},
...rest
                             }: LoaderFunctionArgs) {
    return schemaLoader({...rest,params:{site, slug}});
}

export const action =   ({
                                  params:{site, schema: slug},
                                  ...request
                              }: ActionFunctionArgs) => {
      return schemaAction({...request, params:{site, schema: slug}});
  }

export default function SchemaViewer() {

    const json = useLoaderData();
    const location = useLocation();
    const {site, schema} = location.state;
    const actionData = useActionData<typeof action>();




    return (
        <>
            <form action={`/edit/${site}/${schema}`}>
               <h3>{schema}</h3>
               <Json   src={json} name={"input"}/>
                {actionData?.errors.input ? (
                    <p style={{ color: "red" }}>
                        {actionData.errors.input}
                    </p>
                ) : null}

            </form>
        </>
    )
}
