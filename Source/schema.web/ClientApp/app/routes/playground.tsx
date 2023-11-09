import {Link, Outlet} from '@remix-run/react'
import React, {useState} from "react";
import {Button} from "@mui/base";
import Editors from "~/playground/Editors";
import ErrorBoundary from "~/playground/ErrorBoundary";
import DemoFrame from "~/playground/DemoFrame";
import GeoPosition from "~/playground/GeoPosition";
import SpecialInput from "~/playground/SpecialInput";
import {RJSFValidationError} from "@rjsf/utils";

export default function Playground() {
    return (
        <PlaygroundLayout>
            <Outlet />
        </PlaygroundLayout>
    )
}

function PlaygroundLayout() {
    return (
        <div>
            {/* Layout markup, nav, header etc */}

            <Outlet />

            {/* Layout markup, footer etc */}
        </div>
    )
}

function PlaygroundSetup() {

    const [schema, setSchema] = useState(initialSchema);
    const [uiSchema, setUiSchema] = useState({});
    const [formData, setFormData] = useState({});

    // state for other config: theme, live settings, etc

    return (
        <div>
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
                <ErrorBoundary>
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

                </ErrorBoundary>

            <Link to="/playground/preview">
                <Button>Preview Form</Button>
            </Link>
        </div>
    )
}
