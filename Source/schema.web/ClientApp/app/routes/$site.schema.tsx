import {json, LoaderFunctionArgs} from "@remix-run/node";
import schema from "~/services/schema";
import {Outlet, useLoaderData, useOutletContext} from "@remix-run/react";
import Json from "~/editor";
import React from "react";

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
        const {data } = response;
        console.log("get schema", {
            site,
            data,
            response
        });
        return json( data || mock)

    }catch (e) {
        console.error(e);
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
