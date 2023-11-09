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

type EditorsProps = {
  schema: RJSFSchema;
  setBundledSchema: React.Dispatch<React.SetStateAction<RJSFSchema>>;
  setSchema: React.Dispatch<React.SetStateAction<RJSFSchema>>;
  uiSchema: UiSchema;
  setUiSchema: React.Dispatch<React.SetStateAction<UiSchema>>;
  formData: any;
  setFormData: React.Dispatch<React.SetStateAction<any>>;
  extraErrors: ErrorSchema | undefined;
  setExtraErrors: React.Dispatch<React.SetStateAction<ErrorSchema | undefined>>;
  setShareURL: React.Dispatch<React.SetStateAction<string | null>>;

};

export default function Editors({
                                  extraErrors,
                                  formData,
                                  schema,
                                  uiSchema,
                                  setExtraErrors,
                                  setFormData,
                                  setSchema,
                                  setShareURL,
                                  setUiSchema,
                                  setBundledSchema
                                }: EditorsProps) {

  const refResolver = useMemo(() => new RefResolver(), [schema]);
  schema.$id = schema.$id || "source-schema";

  const [drefSchema, setDrefSchema] = useState({});
  useEffect(() => {
    try {
      refResolver.addSchema(schema);
      refResolver.derefSchema(schema.$id);
      setDrefSchema(refResolver.getDerefSchema(schema.$id));
    } catch (e) {
      console.error(e);
    }
  }, [schema]);

  useEffect(() => {
    onDrefSchemaEdited(drefSchema);

  }, [drefSchema]);

  const onSchemaEdited = useCallback(
    (newSchema) => {
      setSchema(newSchema);
      setShareURL(null);
      refResolver.addSchema(schema);

    },
    [setSchema, setShareURL]
  );

  const onUISchemaEdited = useCallback(
    (newUiSchema) => {
      setUiSchema(newUiSchema);
      setShareURL(null);
    },
    [setUiSchema, setShareURL]
  );


  const onDrefSchemaEdited = useCallback(
    (dref) => {
      setBundledSchema(dref);
      setShareURL(null);

    },
    [setBundledSchema, setShareURL]
  );

  const onFormDataEdited = useCallback(
    (newFormData) => {
      if (
        !isEqualWith(newFormData, formData, (newValue, oldValue) => {
          // Since this is coming from the editor which uses JSON.stringify to trim undefined values compare the values
          // using JSON.stringify to see if the trimmed formData is the same as the untrimmed state
          // Sometimes passing the trimmed value back into the Form causes the defaults to be improperly assigned
          return JSON.stringify(oldValue) === JSON.stringify(newValue);
        })
      ) {
        setFormData(newFormData);
        setShareURL(null);
      }
    },
    [formData, setFormData, setShareURL]
  );

  const onExtraErrorsEdited = useCallback(
    (newExtraErrors) => {
      setExtraErrors(newExtraErrors);
      setShareURL(null);
    },
    [setExtraErrors, setShareURL]
  );

  return (
    <div className={"d-flex"}>
      <div className={"flex-grow-1 me-auto p-2"}>
        <Editor title='JSON Schema' code={toJson(schema)} onChange={onSchemaEdited}/>
      </div>
      <div className={"flex-grow-1 p-2"}>
        <Editor title='Bundled Schema' code={toJson(drefSchema)} onChange={onDrefSchemaEdited}/>
      </div>

      <div className={"flex-grow-1 p-2 ms-auto"}>
        <Editor title='formData' code={toJson(formData)} onChange={onFormDataEdited}/>

      </div>
      <div className='grid-col col justify-content-end'>
        {extraErrors && <Editor title='extraErrors' code={toJson(extraErrors || {})}
                                onChange={onExtraErrorsEdited}/>}
      </div>

    </div>
  );
}

