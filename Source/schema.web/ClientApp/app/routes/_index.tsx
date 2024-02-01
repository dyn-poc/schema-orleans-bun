import { json, type MetaFunction } from "@remix-run/node";
import {NavLink, useLoaderData} from "@remix-run/react";
import { db } from "~/db.server";

export const meta: MetaFunction = () => {
  return [
    { title: "Remix on Bun" },
    { name: "description", content: "Welcome to Remix on Bun!" },
  ];
};
declare type site ={
  apiKey: string
}
export async function loader() {
  return json(db.query(`select $apiKey as apiKey;`).all({$apiKey: '4_DxFqHMTOAJNe9VmFvyO3Uw'} , {$apiKey: '4_1zeDwL2G6qXsS1wRbOrTeA' }));
}


export default function Index() {
  let sites = useLoaderData<site[]>();
  console.log("sites", sites);
console.log(sites);
  return (
    <div style={{ fontFamily: "system-ui, sans-serif", lineHeight: "1.8" }}>
      <h1>Welcome to Remix on Bun!</h1>
      <p>
        This app is using <a href="https://bun.sh">Bun</a> as a runtime.
      </p>
      <hr />
      { sites.map(({apiKey})=> {
          return <NavLink key={apiKey} to={`${apiKey}/schema`}>{apiKey}</NavLink>
      })}



      <p>
        ☝️ That message above me was returned from <code>bun:sqlite</code>{" "}
        running completely in-memory.
      </p>
    </div>
  );
}
