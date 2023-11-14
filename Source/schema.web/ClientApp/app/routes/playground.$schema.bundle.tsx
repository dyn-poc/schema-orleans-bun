import {LoaderFunction} from "@remix-run/router";
import {json, LoaderFunctionArgs} from "@remix-run/node";
import $RefParser  from "@apidevtools/json-schema-ref-parser";
import {useParams, useRouteLoaderData} from "react-router";
import {useFetcher} from "@remix-run/react";
import {JSONSchema7} from "json-schema";

// export  async function loader({ params:{schema: schemaName}
//                                       }: LoaderFunctionArgs) {
//
//     const derf = await $RefParser.dereference();
//     return json(derf);
// }

export const action = async ({request, params, context}: LoaderFunctionArgs) => {
    console.log("action", {request,  params, context});
    let body = await request.text()
    let json = JSON.parse(body)
    console.log("action", {url:request.url, json,  params, context});


    const derf = await $RefParser.dereference(json.$id, json, {
        parse: {
            json: true,
            yaml: true,
        },
        dereference: {
        circular: true
      },
        resolve: {
            file: false,
            http: {
                timeout: 2000,
                withCredentials: true,
            },
            https: true,
            data: true,
            ref: true,
        },
        continueOnError: true,


    });
    return json(derf);
}

export default function BundledSchema() {
    const fetcher = useFetcher();
    const {schema:schemaName} = useParams();
    const schema = useRouteLoaderData(schemaName || "schema");

    return (
        <div className="m-5 p-2">
            <h1 className="text-2xl font-bold">Schema Viewer</h1>
            <pre className="text-xs">{JSON.stringify(schema, null, 2)}</pre>
        </div>
    );
}
