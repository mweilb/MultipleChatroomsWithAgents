import { WebSocketBaseMessage } from "./WebSocketBaseMessage";

export interface WebSocketRequestTranslateMessage extends WebSocketBaseMessage {
    Channel?: string;
    AudioData?: number[]
}
 