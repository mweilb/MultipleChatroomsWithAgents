import { WebSocketBaseMessage } from './WebSocketBaseMessage';


/** Profile of an agent in a chat room */
export interface WebSocektLibrainDocRef {
    /** Agent's name */
    DocumentUri: string;
    Text: string;
    Question: string;
    Answer: string;
    Score: string;
  }
  

export interface WebSocketLibrarianList extends WebSocketBaseMessage {
    Question: string; // Base64 encoded audio data
    RoomName: string;
    AgentName: string;
    References: WebSocektLibrainDocRef[];
}
  