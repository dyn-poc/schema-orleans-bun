import * as monaco from 'monaco-editor';
import editorWorker from 'monaco-editor';
import 'monaco-editor/esm/vs/language/json/json.worker';
// import  'monaco-editor/esm/vs/language/css/css.worker?worker';
// import   'monaco-editor/esm/vs/language/html/html.worker?worker';
// import 'monaco-editor/esm/vs/language/typescript/ts.worker?worker';
//
// export const init=()=> {
//    const worker = monaco.editor.createWebWorker({moduleId: 'vs/language/json/jsonWorker', label: 'json'});
//     return worker;
//
//    if ("serviceWorker" in navigator) {
//       // Use the window load event to keep the page load performant
//       window.addEventListener("load", () => {
//          navigator.serviceWorker
//              .register("/entry.worker.js")
//              .then(() => navigator.serviceWorker.ready)
//              .then(() => {
//                 if (navigator.serviceWorker.controller) {
//                    navigator.serviceWorker.controller.postMessage({
//                       type: "SYNC_REMIX_MANIFEST",
//                       manifest: window.__remixManifest,
//                    });
//                 } else {
//                    navigator.serviceWorker.addEventListener("controllerchange", () => {
//                       navigator.serviceWorker.controller?.postMessage({
//                          type: "SYNC_REMIX_MANIFEST",
//                          manifest: window.__remixManifest,
//                       });
//                    });
//                 }
//              })
//              .catch((error) => {
//                 console.error("Service worker registration failed", error);
//              });
//       });
//    }
//
// }
 // // @ts-ignore
// self.MonacoEnvironment = {
//     getWorker(_: any, label: string) {
//         if (label === 'json') {
//             return new jsonWorker();
//         }
//         if (label === 'css' || label === 'scss' || label === 'less') {
//             return new cssWorker();
//         }
//         if (label === 'html' || label === 'handlebars' || label === 'razor') {
//             return new htmlWorker();
//         }
//         if (label === 'typescript' || label === 'javascript') {
//             return new tsWorker();
//         }
//         return new editorWorker();
//     }
// };

// monaco.languages.typescript.typescriptDefaults.setEagerModelSync(true);
// export default init;
