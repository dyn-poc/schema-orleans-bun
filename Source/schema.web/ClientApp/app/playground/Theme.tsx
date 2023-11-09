import Form, { IChangeEvent } from '@rjsf/core';
import { RJSFSchema, UiSchema } from '@rjsf/utils';
import localValidator from '@rjsf/validator-ajv8';

export interface ThemesType {
    theme: any;
    stylesheet: string;
    subthemes?:{[themeName: string]: SubthemeType}
 }

 export interface SubthemeType {
     stylesheet: string;
 }

interface ThemeSelectorProps {
    theme: string;
    themes: { [themeName: string]: ThemesType };
    select: (themeName: string, theme: ThemesType) => void;
}

export default function ThemeSelector({ theme, themes, select }: ThemeSelectorProps) {
    const schema: RJSFSchema = {
        type: 'string',
        title: 'Theme',
        enum: Object.keys(themes),
    };

    const uiSchema: UiSchema = {
        'ui:placeholder': 'Select theme',
    };

    return (
        <Form
            className='form_rjsf_themeSelector'
            idPrefix='rjsf_themeSelector'
            schema={schema}
            uiSchema={uiSchema}
            formData={theme}
            validator={localValidator}
            onChange={({ formData }: IChangeEvent) => formData && select(formData, themes[formData])}
        >
            <div />
        </Form>
    );
}
