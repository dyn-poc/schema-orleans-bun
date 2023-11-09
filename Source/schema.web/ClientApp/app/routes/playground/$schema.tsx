import React, {useCallback, useEffect, useMemo, useRef, useState} from 'react';
import MonacoEditor, {Monaco} from '@monaco-editor/react';
// import { ErrorSchema, RJSFSchema, UiSchema } from '@rjsf/utils';
import isEqualWith from 'lodash/isEqualWith';

export type ErrorSchema = any;
export type  RJSFSchema = any;
export type UiSchema = any;
import {RefResolver} from "json-schema-ref-resolver";
import {Grid} from '@mui/material';
import Samples, {Sample} from "~/samples";
import {json} from "@remix-run/node";
import {useLoaderData} from "@remix-run/react";
import ajv from "ajv";
import v8Validator, { customizeValidator } from '@rjsf/validator-ajv8';

const monacoEditorOptions = {
    minimap: {
        enabled: false,
    },
    automaticLayout: true,
};

type EditorProps = {
    title: string;
    code: string;
    onChange: (code: string) => void;
};

function Editor({title, code, onChange}: EditorProps) {
    const [valid, setValid] = useState(true);
    const monacoRef = useRef<Monaco>(null);


    const onCodeChange = useCallback(
        (code: string | undefined) => {
            if (!code) {
                return;
            }

            try {
                const parsedCode = JSON.parse(code);
                setValid(true);
                onChange(parsedCode);
            } catch (err) {
                setValid(false);
            }
        },
        [setValid, onChange]
    );

    function handleEditorWillMount(monaco: Monaco) {

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


        monaco.editor.setTheme('vs-dark');


    }

    const icon = valid ? 'ok' : 'remove';
    const cls = valid ? 'valid' : 'invalid';

    return (
        <div className='panel panel-default'>
            <div className='panel-heading'>
                <span className={`${cls} glyphicon glyphicon-${icon}`}/>
                {' ' + title}
            </div>
            <MonacoEditor
                language='json'
                value={code}
                theme='vs-light'
                onChange={onCodeChange}
                height={400}
                options={monacoEditorOptions}
                beforeMount={handleEditorWillMount}
            />
        </div>
    );
}

const toJson = (val: unknown) => JSON.stringify(val, null, 2);


/// create small components to display and edit schema, to display the bundled schema, the demoframe, and to display validation errors
declare type SchemaEditorProps = {
    schema: RJSFSchema;
    onChange: (schema: RJSFSchema) => void;
}

declare type FormDataEditorProps = {
    formData: any;
    schema: RJSFSchema;
    onChange: (formData: any) => void;
}

declare type ErrorsEditorProps = {
    errors?: ErrorSchema;
    onChange?: (errors: ErrorSchema | undefined) => void;
 }
declare type ValidationsProps = {
    schema: RJSFSchema;
    formData: any;
    onChange: (errors: ErrorSchema | undefined) => void;
}
function SchemaEditor({ schema, onChange }:SchemaEditorProps) {
    const onChangeCallback = useCallback(
        (schema: RJSFSchema) => {
            onChange(schema);
        },
        [onChange]);
    return (
        <Editor title='JSON Schema' code={toJson(schema)} onChange={onChangeCallback} />
    );
}
function BundledSchemaEditor({ schema, onChange }:SchemaEditorProps) {
    const refResolver = useMemo(() => new RefResolver(), [schema]);
    const [drefSchema, setDrefSchema] = useState({});
    const onChangeCallback = useCallback(
        (dref: RJSFSchema) => {
             onChange(dref);
        },
        [onChange]);

    useEffect(() => {
        onChangeCallback(drefSchema);
    }, [drefSchema]);


    useEffect(() => {
        try {
            refResolver.addSchema(schema);
            refResolver.derefSchema(schema.$id);
            setDrefSchema(refResolver.getDerefSchema(schema.$id));
        } catch (e) {
            console.error(e);
        }
    }, [schema]);

    return (
        <Editor title='Bundled Schema' code={toJson(schema)} onChange={onChangeCallback} />
    );
}

function FormEditor({ formData, onChange }: FormDataEditorProps) {
    return <Editor title='Form Data' code={toJson(formData)} onChange={onChange} />;
}

function Validations({ formData, schema, onChange }: ValidationsProps) {
    const [errors, setErrors] = useState<ErrorSchema | undefined>(undefined);
    return  <Errors errors={errors} onChange={setErrors} />;
}
function Errors({ errors , onChange }: ErrorsEditorProps) {
    return <> { errors && <Editor title='extraErrors' code={toJson(errors)} onChange={onChange} /> }</>;
}
// combine editors into a remix router
export  function loader({schema}:{schema: "simple" | "communication"}) {
    return json(Samples[schema]);
}

export default function EditorsPage() {
    // @ts-ignore
    const {schema:initialSchema, formData: initialFormData, uiSchema: uiSchema } = useLoaderData<Sample>();
    const [schema, setSchema] = useState({$id: "source-schema", ...initialSchema});
    const [formData, setFormData] = useState(initialFormData);
    const [bundledSchema, setBundledSchema] = useState({});



    return (
        <>
            <Grid container spacing={2}>
                <Grid item xs={3} style={{ height: '100%' }}>
                    <SchemaEditor schema={schema} onChange={setSchema} />
                    <BundledSchemaEditor schema={schema} onChange={setBundledSchema} />
                    <FormEditor schema={bundledSchema} formData={formData} onChange={setFormData} />
                    <Validations schema={schema} formData={formData} onChange={setFormData} />
                </Grid>

            </Grid>
            </>
    );
}
