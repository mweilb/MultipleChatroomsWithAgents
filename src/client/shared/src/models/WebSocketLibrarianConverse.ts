import { WebSocketBaseMessage } from './WebSocketBaseMessage';

export interface WebSocketLibrarianConverse extends WebSocketBaseMessage {
    Question: string; // Base64 encoded audio data
    Thinking: string;
    RoomName: string;
    AgentName: string;
}
  