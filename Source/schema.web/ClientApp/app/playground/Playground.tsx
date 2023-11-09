import { ComponentType, FormEvent, useCallback, useEffect, useRef, useState } from 'react';
import { FormProps, IChangeEvent, withTheme } from '@rjsf/core';
import { ErrorSchema, RJSFSchema, RJSFValidationError, UiSchema, ValidatorType } from '@rjsf/utils';

import { samples, type Sample } from '~/samples';
import Header, { LiveSettings } from '~/playground/Header';
import DemoFrame from '~/playground/DemoFrame';
import ErrorBoundary from '~/playground/ErrorBoundary';
import GeoPosition from '~/playground/GeoPosition';
import type {SubthemeType, ThemesType} from '~/playground/Theme';
import Editors from '~/playground/Editors';
import SpecialInput from '~/playground/SpecialInput';
import {useLocation} from "@remix-run/react";
import {Grid} from "@mui/material";

export interface PlaygroundProps {
    themes: { [themeName: string]: ThemesType};
    validators: { [validatorName: string]: ValidatorType };
}


export default function Playground({ themes, validators }: PlaygroundProps) {
    const [loaded, setLoaded] = useState(false);
    const [schema, setSchema] = useState<RJSFSchema>(samples.simple.schema as RJSFSchema);
  const [bundledSchema, setBundledSchema] = useState<RJSFSchema>(samples.simple.schema as RJSFSchema);

  const [uiSchema, setUiSchema] = useState<UiSchema>(samples.simple.uiSchema);
    const [formData, setFormData] = useState<any>(samples.simple.formData);
    const [extraErrors, setExtraErrors] = useState<ErrorSchema | undefined>();
    const [shareURL, setShareURL] = useState<string | null>(null);
    const [theme, setTheme] = useState<string>('default');
    const [subtheme, setSubtheme] = useState<string | null>(null);
    const [stylesheet, setStylesheet] = useState<string | null>(null);
    const [validator, setValidator] = useState<string>('AJV8');
    const [showForm, setShowForm] = useState(false);
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
    const [FormComponent, setFormComponent] = useState<ComponentType<FormProps>>(withTheme({}));
    const [otherFormProps, setOtherFormProps] = useState<Partial<FormProps>>({});

    const playGroundFormRef = useRef<any>(null);

    const onThemeSelected = useCallback(
        (theme: string, { stylesheet, theme: themeObj }: ThemesType) => {
            setTheme(theme);
            setSubtheme(null);
            setFormComponent(withTheme(themeObj));
            setStylesheet(stylesheet);
        },
        [setTheme, setSubtheme, setFormComponent, setStylesheet]
    );

    const load = useCallback(
        (data: Sample & { theme: string; liveSettings: LiveSettings, bundledSchema?: RJSFSchema }) => {
            const {
                schema,
                bundledSchema,
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

            onThemeSelected(dataTheme, themes[dataTheme]);

            // force resetting form component instance
            setShowForm(false);
            setSchema(schema);
            setBundledSchema(bundledSchema || {});
            setUiSchema(uiSchema);
            setFormData(formData);
            setExtraErrors(extraErrors);
            setTheme(dataTheme);
            setShowForm(true);
            setLiveSettings(liveSettings);
            setOtherFormProps({ fields, templates, ...rest });
        },
        [theme, onThemeSelected, themes]
    );
    const location = useLocation();
    useEffect(() => {
        const hash = location.hash.match(/#(.*)/);

        if (hash && typeof hash[1] === 'string' && hash[1].length > 0 && !loaded) {
            try {
                load(JSON.parse(atob(hash[1])));
                setLoaded(true);
            } catch (error) {
                alert('Unable to load form setup data.');
                console.error(error);
            }

            return;
        }

        // initialize theme
        onThemeSelected(theme, themes[theme]);

        setShowForm(true);
    }, [onThemeSelected, load, loaded, setShowForm, theme, themes]);

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

    return (
        <Grid container spacing={2}  >
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
                setBundledSchema={setBundledSchema}

            />
            <Grid item xs={3} style={{ height: '100%' }}>
                <ErrorBoundary>
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
                                schema={bundledSchema}
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
                </ErrorBoundary>
            </Grid>
        </Grid>
    );
}
/// create small components to display and edit schema, to display the bundled schema, the demoframe, and to display validation errors
const SchemaDisplay = ({schema}) => {
    return <pre>{JSON.stringify(schema, null, 2)}</pre>
}
const SchemaEditor = ({schema, setSchema}) => {
    return <pre>{JSON.stringify(schema, null, 2)}</pre>
}
const SchemaBundledDisplay = ({schema}) => {
    return <pre>{JSON.stringify(schema, null, 2)}</pre>
}

const ValidationErrorsDisplay = ({errors}) => {
    return <pre>{JSON.stringify(errors, null, 2)}</pre>
}


