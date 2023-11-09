import {ActionFunctionArgs, json, LoaderFunctionArgs, MetaFunction} from "@remix-run/node";
import schema from "~/services/schema";
import {Form, Outlet, useActionData, useLoaderData, useLocation, useOutletContext} from "@remix-run/react";
import Json from "~/editor";
import React from "react";
import axios from "axios";
import type { loader as siteLoader } from "./$site";
export const meta: MetaFunction<
    typeof loader,
    { "routes/$site": typeof siteLoader }
> = ({ data, matches }) => {
    const site = matches.find(
        (match) => match.id === "routes/$site"
    );
    console.log("meta", {site, data, matches});
    return [{ title: data.title, description: data.description }];
};

// export const meta: MetaFunction = ({location}) => {
//
//     const {href, absolute, ref}= location.state;
//
//     console.log("meta", {href, absolute, ref, location});
//     return {
//         title: "Schema Viewer"
//     };
// }

export async function loader({
                                 params:{site, schema: schemaName},

                             }: LoaderFunctionArgs) {
    try{
        const response =  await schema(`${site}/${schemaName}`);
        const {data } = response;
        console.log("get schema", {
            site,
            schemaName,
            data,
            response
        });
        return json( data || {status: response.statusText})

    }catch (e) {
        console.error(e);
        return json( {error: e})
    }
}


export const action = async ({
                                 params:{site, type},
                                 request,
                             }: ActionFunctionArgs) => {

    const formData = await request.formData();
    const response = await schema(`${site}/${type}`, {
        method: "POST",
        data: formData.get("input"),
        headers: {
            "Content-Type": "application/json",
        },
    });

    const {data} = response;
    console.log("response", {
        status_code: response.status,
        status_text: response.statusText,
        headers: JSON.stringify(response.headers, null, 2),
        data: response.data
    });
    return json(data);

 };

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
