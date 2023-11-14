import {json, LoaderFunctionArgs, MetaFunction} from "@remix-run/node";
import schema from "~/services/schema";
import {Outlet, useLoaderData, useLocation, useOutletContext} from "@remix-run/react";
import Json from "~/editor";
import React from "react";
import axios from "axios";
import type { loader as siteLoader } from "./$site";

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

export const meta: MetaFunction<
    typeof loader,
    { "routes/$site": typeof siteLoader }
> = ({ data, matches }) => {
    const site = matches.find(
        (match) => match.id === "routes/$site"
    );
    // console.log("meta", {site, data, matches});
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
        console.log(`GET ${site}/${schemaName}:` , {
            status_code: response.status,
            status_text: response.statusText,
            headers: JSON.stringify(response.headers, null, 2),
            data: response.data
        });
        return json( data || mock)

    }catch (e) {
        const {stack, message} = e as Error;
        console.error(`Failed to GET ${site}/${schemaName}`,{stack, message});
        return json( mock)
    }
}




export default function SchemaViewer() {

    const json = useLoaderData();



    return (
        <>
            <div className="m-5 p-2" >

                {/*<Outlet context={{site}} />*/}

                <Json   src={json}/>

            </div>
        </>
    )
}
