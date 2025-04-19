import { WebSocketBaseMessage } from './WebSocketBaseMessage';

export interface WebSocketReplyChatRoomMessage extends WebSocketBaseMessage {
  AgentName: string;
  DisplayName: string;
  Emoji: string;
  Orchestrator: string;
  RoomName: string;
}
