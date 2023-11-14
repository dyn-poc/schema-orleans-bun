import React, {ComponentType, useCallback, useEffect, useMemo, useRef, useState} from 'react';
import MonacoEditor, {Monaco} from '@monaco-editor/react';
// import { ErrorSchema, RJSFSchema, UiSchema } from '@rjsf/utils';
import isEqualWith from 'lodash/isEqualWith';

export type ErrorSchema = any;
export type  RJSFSchema = any;
export type UiSchema = any;
import {RefResolver} from "json-schema-ref-resolver";
import {Grid} from '@mui/material';
import Samples, {Sample} from "~/samples";
import {json, LoaderFunctionArgs} from "@remix-run/node";
import {useFetcher, useLoaderData, useSubmit} from "@remix-run/react";
import ajv from "ajv";
import v8Validator, {customizeValidator} from '@rjsf/validator-ajv8';
import GeoPosition from "~/playground/GeoPosition";
import SpecialInput from "~/playground/SpecialInput";
import {RJSFValidationError} from "@rjsf/utils";
import DemoFrame from "~/playground/DemoFrame";
import {themes} from "~/playground/Default";
import {FormProps, withTheme} from "@rjsf/core";
import {isAbsolute} from "pathe";
import {isUrl} from "vfile/lib/minurl.shared";
import $RefParser from "@apidevtools/json-schema-ref-parser";

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
    const [data, setData] = useState(code);

    const onCodeChange = useCallback(
        (code: string | undefined) => {
            if (!code) {
                return;
            }
            setData(code);


        },
        [setValid, onChange]
    );
    const onChangeCallback = useCallback(
        (code: string) => {
            try {
                const parsedCode = JSON.parse(code);
                setValid(true);
                onChange(parsedCode);
            } catch (err) {
                setValid(false);
            }
        },
        [onCodeChange]);

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
                <button
                    type='button'
                    className='nav-link btn btn-info btn-xs top-nav'
                    onClick={() => onChangeCallback(data)} >
                    Update
                </button>

            </div>
            <MonacoEditor
                language='json'
                value={data}
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
    onChange?: (schema: RJSFSchema) => void;
    className?: string;
}

declare type FormDataEditorProps = {
    formData: any;
    schema: RJSFSchema;
    onChange?: (formData: any) => void;
    className?: string;
}

declare type ErrorsEditorProps = {
    errors?: ErrorSchema;
    onChange?: (errors?: ErrorSchema | undefined) => void;
    className?: string;
}
declare type ValidationsProps = {
    schema: RJSFSchema;
    formData: any;
    onChange: (errors: ErrorSchema | undefined) => void;
    className?: string;
}

function SchemaEditor({schema, onChange, className}: SchemaEditorProps) {
    const submit = useSubmit();
    const onChangeCallback = useCallback(
        (schema: RJSFSchema) => {
            // submit(schema, {action: "bundle", method: "POST", encType: "application/json"});
           typeof onChange === "function" &&  onChange(schema);

        },
        [onChange]);
    return (
        // <div className={className}>
        //     <div className='panel panel-default'>
        //         <div className='panel-heading nav-bar nav'>
        //              <span className='pull-right'>
        //                 <button
        //                     type='button'
        //                     className='nav-link btn btn-info btn-xs top-nav'
        //                     onClick={() => onChangeCallback(newSchema)} >
        //                     Update Schema
        //                 </button>
        //             </span>
        //         </div>
        //     <Editor title='JSON Schema' code={toJson(newSchema)} onChange={setNewSchema}/>
        //     </div>
        // </div>
        <div className={className}>
            <Editor title='JSON Schema' code={toJson(schema)} onChange={onChangeCallback}/>
        </div>
    );
}

async function addExternalSchemas(  refResolver: RefResolver, schema: RJSFSchema) {
   const refs=refResolver.getSchemaRefs(schema.$id);

    const filter= (ref: { schemaId: string; }) => !refResolver.hasSchema(ref.schemaId) && (ref.schemaId.startsWith("https://") || ref.schemaId.startsWith("http://"));
    const filteredRefs=refs.filter( filter);
    for (const {schemaId} of filteredRefs) {
        refResolver.addSchema(await (await fetch(schemaId)).json());
        // refResolver.derefSchema(schemaId);
    }
}


async function drefSchemaAsync(refResolver: RefResolver, schema: RJSFSchema) {
    try {
        refResolver.addSchema(schema);
        await addExternalSchemas(refResolver, schema);
        refResolver.derefSchema(schema.$id);
        const {$defs, ...rest} = refResolver.getDerefSchema(schema.$id);
        return rest;
    }
    catch (e) {
        console.error(e);
        return e;
    }
}
 function dref(refResolver: RefResolver, schema: RJSFSchema) {
    try {
        refResolver.addSchema(schema);
        refResolver.derefSchema(schema.$id);
        const {$defs, ...rest} = refResolver.getDerefSchema(schema.$id);
        return rest;
    }
    catch (e) {
        console.error(e);
        return e;
    }
}

function BundledSchemaEditor({schema, onChange, className}: SchemaEditorProps) {
    const refResolver = useMemo(() => new RefResolver(), [schema]);

    const [drefSchema, setDrefSchema] = useState();
    const onChangeCallback = useCallback(
        (dref: RJSFSchema) => {
            onChange(dref);
        },
        [onChange]);

    useEffect(() => {
        onChangeCallback(drefSchema);
    }, [drefSchema]);


    useEffect(() => {
      setDrefSchema(dref(refResolver, schema));
    }, [schema, refResolver]);

    return (

        <div className={className}>
            <Editor title='Bundled Schema' code={toJson(drefSchema || {})} onChange={onChangeCallback}/>
        </div>
    );
}

function FormEditor({formData, onChange, className}: FormDataEditorProps) {
    const onChangeCallback = useCallback(
        (code: any) => {
            console.log(code)
           typeof onChange === "function" && onChange(code );
        },
        [onChange]);

    return <div className={className}>
        <Editor title='Form Data' code={toJson(formData)} onChange={onChangeCallback}/>;
    </div>
}

function Validations({formData, schema, onChange, className}: ValidationsProps) {
    const [errors, setErrors] = useState<ErrorSchema | undefined>(undefined);
    return <div className={className}>
        <Errors errors={errors} onChange={setErrors}/>
    </div>;
}



function Form({schema, uiSchema, formData, onChange, stylesheet, ...rest}: any) {
    const [FormComponent, setFormComponent] = useState<ComponentType<FormProps>>(withTheme({}));

    return <>{schema && <DemoFrame
        head={
            <>
                <link rel='stylesheet' id='theme' href={stylesheet ||  themes.default.stylesheet}/>
            </>
        }
        style={{
            width: '100%',
            height: 1000,
            border: 0,
        }}
        theme={"default"}
    >
        <FormComponent
            schema={schema}
            uiSchema={uiSchema}
            formData={formData}
            onChange={onChange}
            validator={v8Validator}
            fields={{
                geo: GeoPosition,
                '/schemas/specialString': SpecialInput,
            }}
            theme={themes.default}
            {...rest}

            onBlur={(id: string, value: string) => console.log(`Touched ${id} with value ${value}`)}
            onFocus={(id: string, value: string) => console.log(`Focused ${id} with value ${value}`)}
            onError={(errorList: RJSFValidationError[]) => console.log('errors', errorList)}
        />
    </DemoFrame> }</>
}

function Errors({errors, onChange, className}: ErrorsEditorProps) {
    const onChangeCallback = useCallback(
        (code: string) => {
            onChange(JSON.parse(code) as ErrorSchema || {});
        },
        [onChange]);

    return <> {errors && <div className={className}>
        <Editor title='extraErrors' code={toJson(errors)} onChange={onChangeCallback}  />
    </div>}</>;
}

// combine editors into a remix router
export async function loader({
                                 params: {schema}

                             }: LoaderFunctionArgs) {
    console.log("loader schema", schema);
    const sample = schema as "simple" || "communication";
    return json( {$id: schema, ...Samples[sample]});

}

export const action = async ({request, params, context}: LoaderFunctionArgs) => {
    let body = await request.text()
    let json = JSON.parse(body)
    console.log("action", {url:request.url, json,  params, context});


    const derf = await $RefParser.dereference(json.$id, json, {
        parse: {
            json: true,
            yaml: true,
        },
        dereference: {
            circular: true
        },
        resolve: {
            file: false,
            http: {
                timeout: 2000,
                withCredentials: true,
            },
            https: true,
            data: true,
            ref: true,
        },
        continueOnError: true,


    });
    return json(derf);
}

export default function EditorsPage() {
    // @ts-ignore
    const {schema: initialSchema, formData: initialFormData, uiSchema: uiSchema} = useLoaderData<Sample>();
    const [schema, setSchema] = useState(initialSchema);
    const [formData, setFormData] = useState(initialFormData);
    const [bundledSchema, setBundledSchema] = useState({});
    const [formState, setFormState] = useState({});
    const bundle=useFetcher();
    const submit = useSubmit();
    // const submitSchema = useCallback(
    //     (schema: RJSFSchema) => {
    //         setSchema(schema);
    //         bundle.submit(schema, { method: "POST", encType: "application/json", navigate:false});
    //     },
    //     [bundle]);

    useEffect(() => {
        submit(schema, { method: "POST", encType: "application/json", navigate:false})
    }, [schema]);
    useEffect(() => {
        console.log("formState", formState);
    }, [formState]);

    useEffect(() => {
        console.log("EditorsPage initialSchema", initialSchema);
    }, [initialSchema]);

    useEffect(() => {
        console.log("EditorsPage schema", schema);
    }, [schema]);
    useEffect(() => {
        console.log("EditorsPage bundle.data", bundle.data);
    }, [bundle.data]);
    return (
        <div className={"d-flex flex-row flex-wrap align-content-stretch w-100 p-3 justify-content-between"}
             style={{backgroundColor: "#eee"}}>
            <SchemaEditor schema={schema} onChange={setSchema} className={"w-100"}/>
            <SchemaEditor schema={bundle.data} className={"w-50"}  />
            <BundledSchemaEditor schema={schema} onChange={setBundledSchema} className={"w-50"}/>
            <FormEditor schema={bundledSchema} formData={formData} onChange={setFormData}  className={"w-50"}/>
            <Validations schema={schema} formData={formData} onChange={setFormState}/>
            <Form schema={bundledSchema} uiSchema={uiSchema} formData={formData} onChange={setFormState} />
        </div>
    );
}
