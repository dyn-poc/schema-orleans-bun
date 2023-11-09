import Simple from './simple';
import {JSONSchema7} from "json-schema";
import communication from "~/samples/communication";



export const samples ={
    simple: Simple,
    communication: communication,
}

export type Samples = keyof typeof samples;
export * from './Sample'
export default samples;

// export type Sample = {
//     schema: any;
//     uiSchema?:any;
// }
