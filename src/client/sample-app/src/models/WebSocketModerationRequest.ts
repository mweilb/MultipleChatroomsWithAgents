import { WebSocketBaseMessage } from "./WebSocketBaseMessage";




export interface WebSocketModeration extends WebSocketBaseMessage {
    /** The room's name */
    Why: string;
}

 