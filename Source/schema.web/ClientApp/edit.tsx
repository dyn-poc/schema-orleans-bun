import Editors, {ErrorSchema, RJSFSchema, UiSchema} from "~/editor/Editors";
import {ComponentType, FormEvent, useCallback, useRef, useState} from "react";

import samples from '~/samples';
import {FormProps, IChangeEvent, withTheme} from "@rjsf/core";
import DemoFrame from "~/components/DemoFrame";
import Header, {LiveSettings} from "~/Playground/Header";
import {ValidatorType} from "@rjsf/utils";
import {ThemesType} from "~/Playground/Theme";
import localValidator from "@rjsf/validator-ajv8";
import {useLoaderData} from "@remix-run/react";
declare  type Sample = any;

export interface PlaygroundProps {
    themes: { [themeName: string]: ThemesType };
    validators: { [validatorName: string]: ValidatorType };
}

export async function loader():Promise<PlaygroundProps> {

    return {
    themes: {
      default: {
        theme: {},
        stylesheet: ""
      }
    },
    validators: {
      "AJV8": localValidator
    }
  }
}

export default function SchemaViewer() {
    const [loaded, setLoaded] = useState(false);
    const [schema, setSchema] = useState<RJSFSchema>(samples.simple.schema as RJSFSchema);
    const [uiSchema, setUiSchema] = useState<UiSchema>(samples.simple.uiSchema);
    const [formData, setFormData] = useState<any>(samples.simple.formData);
    const [extraErrors, setExtraErrors] = useState<ErrorSchema | undefined>();
    const [shareURL, setShareURL] = useState<string | null>(null);
    const [theme, setTheme] = useState<string>('default');
    const [subtheme, setSubtheme] = useState<string | null>(null);
    const [stylesheet, setStylesheet] = useState<string | null>(null);
    const [validator, setValidator] = useState<string>('AJV8');
    const [showForm, setShowForm] = useState(false);
    const [FormComponent, setFormComponent] = useState<ComponentType<FormProps>>(withTheme({}));
    const [otherFormProps, setOtherFormProps] = useState<Partial<FormProps>>({});
    const playGroundFormRef = useRef<any>(null);
    const { themes, validators } = useLoaderData<typeof loader>();
    const [liveSettings, setLiveSettings] = useState<LiveSettings>({
        showErrorList: 'top',
        validate: false,
        disabled: false,
        noHtml5Validate: false,
        readonly: false,
        omitExtraData: false,
        liveOmit: false,
        experimental_defaultFormStateBehavior: { arrayMinItems: 'populate', emptyObjectFields: 'populateAllDefaults' },
    });


    const load = useCallback(
        (data: Sample & { theme: string;  }) => {
            const {
                schema,
                // uiSchema is missing on some examples. Provide a default to
                // clear the field in all cases.
                uiSchema = {},
                // Always reset templates and fields
                templates = {},
                fields = {},
                formData,
                theme: dataTheme = theme,
                extraErrors,
                liveSettings,
                ...rest
            } = data;


            // force resetting form component instance
            setShowForm(false);
            setSchema(schema);
            setUiSchema(uiSchema);
            setFormData(formData);
            setExtraErrors(extraErrors);
            setTheme(dataTheme);
            setShowForm(true);
         },
        [theme])

    const onFormDataChange = useCallback(
        ({ formData }: IChangeEvent, id?: string) => {
            if (id) {
                console.log('Field changed, id: ', id);
            }

            setFormData(formData);
            setShareURL(null);
        },
        [setFormData, setShareURL]
    );

    const onFormDataSubmit = useCallback(({ formData }: IChangeEvent, event: FormEvent<any>) => {
        console.log('submitted formData', formData);
        console.log('submit event', event);
     }, []);


    return  <>
        <Header
            schema={schema}
            uiSchema={uiSchema}
            formData={formData}
            shareURL={shareURL}
            themes={themes}
            theme={theme}
            subtheme={subtheme}
            validators={validators}
            validator={validator}
            liveSettings={liveSettings}
            playGroundFormRef={playGroundFormRef}
            load={load}
            onThemeSelected={onThemeSelected}
            setSubtheme={setSubtheme}
            setStylesheet={setStylesheet}
            setValidator={setValidator}
            setLiveSettings={setLiveSettings}
            setShareURL={setShareURL}
        />
        <Editors
        formData={formData}
        setFormData={setFormData}
        schema={schema}
        setSchema={setSchema}
        uiSchema={uiSchema}
        setUiSchema={setUiSchema}
        extraErrors={extraErrors}
        setExtraErrors={setExtraErrors}
        setShareURL={setShareURL}
    />
        <div className='col-sm-5'>
                 {showForm && (
                    <DemoFrame
                        head={
                            <>
                                <link rel='stylesheet' id='theme' href={stylesheet || ''} />
                            </>
                        }
                        style={{
                            width: '100%',
                            height: 1000,
                            border: 0,
                        }}
                        theme={theme}
                    >
                        <FormComponent
                            {...otherFormProps}
                            {...liveSettings}
                            extraErrors={extraErrors}
                            schema={schema}
                            uiSchema={uiSchema}
                            formData={formData}
                            fields={{
                                geo: GeoPosition,
                                '/schemas/specialString': SpecialInput,
                            }}
                            validator={validators[validator]}
                            onChange={onFormDataChange}
                            onSubmit={onFormDataSubmit}
                            onBlur={(id: string, value: string) => console.log(`Touched ${id} with value ${value}`)}
                            onFocus={(id: string, value: string) => console.log(`Focused ${id} with value ${value}`)}
                            onError={(errorList: RJSFValidationError[]) => console.log('errors', errorList)}
                            ref={playGroundFormRef}
                        />
                    </DemoFrame>
                )}
         </div>
        </>;
}
