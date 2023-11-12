import { FormProps } from '@rjsf/core';
import AJV8Validator from "@rjsf/validator-ajv8/lib/validator";
import {JSONSchema7Version} from "json-schema";
import {RJSFSchema} from "@rjsf/utils";

export type Sample = Omit<FormProps, 'validator'>;
