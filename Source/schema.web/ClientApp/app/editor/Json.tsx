import React, {useEffect, useRef} from "react";
import Editor, {loader, type Monaco, useMonaco} from '@monaco-editor/react';

type JsonProps = {
    src: any,
    schema?: any
};


export default function MonacoJson({src}: JsonProps) {

    const [model, setModel] = React.useState<any>(undefined as any);
    const [schema, setSchema] = React.useState<any>(undefined as any);
    const editorRef = useRef(null);

    const monacoRef = useRef<Monaco>(null);


    useEffect(() => {
        editorRef.current?.focus();

    }, [editorRef.current]);


    function setModelInMonaco(monaco: Monaco) {
        const modelUri = monaco.Uri.parse(src.$id);
        let model = monaco.editor.getModel(modelUri);
        if (!model) {
            model = monaco.editor.createModel(JSON.stringify(src, null, 4), "json", modelUri);
            monaco.editor.setModelLanguage(model, 'json');
            console.log("monaco-effect", model);
        }

        setModel({...model, uri: modelUri});
    }

    useEffect(() => {

        const {current: monaco} = monacoRef;
        if (monaco) {
            setModelInMonaco(monaco);

        }
    }, [src, monacoRef.current]);

    function handleEditorWillMount(monaco) {
        monacoRef.current = monaco;

        // here is the monaco instance
        // do something before editor is mounted
        console.log('handleEditorWillMount', monaco.languages.json.jsonDefaults);
        monaco.languages.typescript.javascriptDefaults.setEagerModelSync(true);
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

        setModelInMonaco(monaco);


        monaco.editor.setTheme('vs-dark');


    }

    function handleEditorValidation(markers: any[]) {
        // model markers
        markers.forEach((marker) => console.log('onValidate:', marker.message));
    }

    function handleEditorDidMount(editor: any, monaco: Monaco) {
        // here is another way to get monaco instance
        // you can also store it in `useRef` for further usage
        editorRef.current = editor;
        monacoRef.current = monaco;

    }


    return (<Editor
            height="90vh"
            loading={"Loading..."}
            defaultLanguage="json"
            beforeMount={handleEditorWillMount}
            onMount={handleEditorDidMount}
            onValidate={handleEditorValidation}
            path={model?.uri}


        />
    )
}

// defaultValue={JSON.stringify(src, undefined, 4)}

function getOptions() {
    return {
        json: true,
        readOnly: true,
        jsonDefaults: {
            schemas: [
                {
                    uri: "http://myserver/bar-schema.json", // id of the second schema
                    schema: {
                        type: "object",
                        properties: {
                            q1: {
                                enum: ["x1", "x2"],
                            },
                        },
                    },
                },
            ]
        }
    };
}
