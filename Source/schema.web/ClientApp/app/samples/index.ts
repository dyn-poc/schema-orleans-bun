import Simple from './simple';
import communication from "~/samples/communication";
import channel_dependencies from "~/samples/channel_dependencies";



export const samples ={
    simple: Simple,
    communication: communication,
    channel: channel_dependencies
}

export type Samples = keyof typeof samples;
export * from './Sample'
export default samples;

