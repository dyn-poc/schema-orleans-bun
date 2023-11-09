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
import {useLoaderData} from "@remix-run/react";
import ajv from "ajv";
import v8Validator, {customizeValidator} from '@rjsf/validator-ajv8';
import GeoPosition from "~/playground/GeoPosition";
import SpecialInput from "~/playground/SpecialInput";
import {RJSFValidationError} from "@rjsf/utils";
import DemoFrame from "~/playground/DemoFrame";
import {themes} from "~/playground/Default";
import {FormProps, withTheme} from "@rjsf/core";

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
    const onChangeCallback = useCallback(
        (schema: RJSFSchema) => {
            onChange(schema);
        },
        [onChange]);
    return (
        <div className={className}>
            <Editor title='JSON Schema' code={toJson(schema)} onChange={onChangeCallback}/>
        </div>);
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
        try {
            refResolver.addSchema(schema);
            refResolver.derefSchema(schema.$id);
            setDrefSchema(refResolver.getDerefSchema(schema.$id));
        } catch (e) {
            console.error(e);
        }
    }, [schema]);

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

    const sample = schema as "simple" || "communication";
    return json(Samples[sample]);

}

export default function EditorsPage() {
    // @ts-ignore
    const {schema: initialSchema, formData: initialFormData, uiSchema: uiSchema} = useLoaderData<Sample>();
    const [schema, setSchema] = useState({$id: "source-schema", ...initialSchema});
    const [formData, setFormData] = useState(initialFormData);
    const [bundledSchema, setBundledSchema] = useState({});
    const [formState, setFormState] = useState({});


    return (
        <div className={"d-flex flex-row flex-wrap align-content-stretch w-100 p-3 justify-content-between"}
             style={{backgroundColor: "#eee"}}>
            <SchemaEditor schema={schema} onChange={setSchema} className={"w-100"}/>
            <BundledSchemaEditor schema={schema} onChange={setBundledSchema} className={"w-50"}/>
            <FormEditor schema={bundledSchema} formData={formData} onChange={setFormData}  className={"w-50"}/>
            <Validations schema={schema} formData={formData} onChange={setFormState}/>

            <Form schema={bundledSchema} uiSchema={uiSchema} formData={formData} onChange={setFormState} />
        </div>
    );
}
