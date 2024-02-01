import {json, LoaderFunctionArgs, MetaFunction} from "@remix-run/node";
import schema from "~/services/schema";
import {Outlet, useFetcher, useLoaderData, useLocation, useOutletContext, useParams} from "@remix-run/react";
import Json from "~/editor";
import React from "react";
import axios from "axios";
import type { loader as siteLoader } from "./$site";



// export const meta: MetaFunction = ({location}) => {
//
//     const {href, absolute, ref}= location.state;
//
//     console.log("meta", {href, absolute, ref, location});
//     return {
//         title: "Schema Viewer"
//     };
// }

import {loader as schemaLoader} from './$site.schema.$';

export async function loader(args: LoaderFunctionArgs) {
    return schemaLoader(args);
}




export default function SchemaViewer() {

    return (
        <>
            <div className="m-5 p-2" >

                {/*<Outlet context={{site}} />*/}

                <Json   src={useLoaderData<typeof loader>()}/>

            </div>
        </>
    )
}
