import { WebSocketBaseMessage } from './WebSocketBaseMessage';

 
/** Profile of an agent in a chat room */
export interface WebSocketAgentProfile {
  /** Agent's name */
  Name: string;
  /** Agent's emoji */
  Emoji: string;
}

/** Profile of a room in a chat with its associated agents */
export interface WebSocketRoomProfile {
  /** The room's name */
  Name: string;
  /** Room's Emoji */
  Emoji: string;
  /** List of agents in the room */
  Agents: WebSocketAgentProfile[];
}

/** Validation error with location and position info */
export interface ValidationError {
  Message: string;
  Location: string;
  LineNumber?: number;
  CharPosition?: number;
}

/** WebSocket message for a chat room with its agents */
export interface WebSocketRoom extends WebSocketBaseMessage {
    /** The room's name */
    Name: string;
    /** Room's Emoji */
    Emoji: string;
    /** Auto Start */
    AutoStart: string;
     /** Graph for Mermaid */
    MerMaidGraph: string;
    /** Raw Yaml Text */
    Yaml: string;
    /** Errors for Yaml **/
    Errors: ValidationError[];
    /** Agents in the room */
    Rooms: WebSocketRoomProfile[];
}



/** WebSocket message carrying a list of chat rooms */
export interface WebSocketGetRoomsMessage extends WebSocketBaseMessage {
  Rooms?: WebSocketRoom[];
}
