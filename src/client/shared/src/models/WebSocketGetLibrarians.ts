import { WebSocketBaseMessage } from './WebSocketBaseMessage';

 
/** Profile of an agent in a chat room */
export interface WebSocketLibrarian {
  /** Agent's name */
  Name: string;
  /** Agent's emoji */
  Emoji: string;
}

/** Profile of a room in a chat with its associated agents */
export interface WebSocketLibrariansRoom {
  /** The room's name */
  Name: string;
  /** Room's Emoji */
  Emoji: string;
  /** List of agents in the room */
  ActiveLibrarians: WebSocketLibrarian[];
  NotActiveLibrarians: WebSocketLibrarian[];
}

/** WebSocket message carrying a list of chat rooms */
export interface WebSocketLibrariansMessage extends WebSocketBaseMessage {
  Rooms?: WebSocketLibrariansRoom[];
}
