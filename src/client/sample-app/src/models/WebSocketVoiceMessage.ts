import { WebSocketBaseMessage } from './WebSocketBaseMessage';

export interface WebSocketAudioMessage extends WebSocketBaseMessage {
    AudioData: string; // Base64 encoded audio data
    AudioFormat: string;
    SampleRate: number;
}
  