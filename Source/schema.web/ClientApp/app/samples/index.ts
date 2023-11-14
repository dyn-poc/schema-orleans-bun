import Simple from './simple';
import communication from "~/samples/communication";
import channel_dependencies from "~/samples/channel_dependencies";
import {refs} from "~/samples/refs";



export const samples ={
    simple: Simple,
    communication: communication,
    channel: channel_dependencies,
    refs: refs
}

export type Samples = keyof typeof samples;
export * from './Sample'
export default samples;

