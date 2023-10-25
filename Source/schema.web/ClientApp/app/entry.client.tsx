import { RemixBrowser } from "@remix-run/react";
import { startTransition, StrictMode } from "react";
import { hydrateRoot } from "react-dom/client";
// import init from "~/editor/init";
// import {languages} from "monaco-editor";
// import getJavaScriptWorker = languages.typescript.getJavaScriptWorker;
//
//


startTransition(() => {
    hydrateRoot(
        document,
        <StrictMode>
            <RemixBrowser />
        </StrictMode>
    );
});
