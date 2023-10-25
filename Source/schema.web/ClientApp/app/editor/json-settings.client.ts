import { loader } from '@monaco-editor/react';


export function configure({src, schema}:JsonProps ) {
    return loader.init().then(async (monaco) => {
        console.log("monaco-init");
        console.log(monaco.editor.getModels());
        console.log(monaco.languages.json.jsonDefaults.diagnosticsOptions);
        console.log(monaco.languages.json.jsonDefaults.diagnosticsOptions.schemas);
        monaco.languages.json.jsonDefaults.setDiagnosticsOptions({
                validate: true,
                allowComments:
                    false, schemas: [],
                enableSchemaRequest: true,
                schemaRequest: "warning",
                schemaValidation: "warning",
                trailingCommas: "warning"
            }
           );
        // monaco.languages.json.jsonDefaults.setModeConfiguration({
        //     documentSymbols:true,
        //     documentFormattingEdits:true,
        //     documentRangeFormattingEdits:true,
        //     foldingRanges:true,
        //     selectionRanges:true,
        //     completionItems:true,
        //     hovers: true,
        //     diagnostics: true,
        // });



        const modelUri = monaco.Uri.parse(src.$id ||  "inmemory://model.json");
        if(!monaco.editor.getModel(modelUri)) {
            const model = monaco.editor.createModel(JSON.stringify(src, null ,4), "json", modelUri);
            monaco.editor.setModelLanguage(model, 'json');
            monaco.editor.setTheme('vs-dark');
            // monaco.languages.json.jsonDefaults.setModeConfiguration({
            //     documentSymbols:true,
            //     documentFormattingEdits:true,
            //     documentRangeFormattingEdits:true,
            //     foldingRanges:true,
            //     selectionRanges:true,
            //      completionItems:true,
            //     hovers: true,
            //     diagnostics: true,
            //
            //
            //
            // });

        }
        if(schema){
            monaco.languages.json.jsonDefaults.setDiagnosticsOptions(
                {
                    validate: true,
                    enableSchemaRequest:true,
                    schemaValidation: "warning",
                    schemaRequest: "warning",



                    schemas:  [
                        {
                            // fileMatch: ["*"], // associate with any file

                            uri: src.$schema || schema.$id,
                            schema: schema,
                        }
                    ]

                }
            );

        }

        console.log(monaco.editor.getModels());


        return modelUri;


    });

}

type JsonProps = {
    uri:string,
    src: any,
    schema?: any
};
