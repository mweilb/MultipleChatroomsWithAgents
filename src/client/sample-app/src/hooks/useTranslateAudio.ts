import axios from "axios";
import { marked } from "marked";
import { htmlToText } from "html-to-text";

export interface TranscriptionResponse {
  text: string;
  // Additional fields can be added as needed.
}

export function useTranslateAudio() {
  // Cast environment variables to string.
  const VITE_AZURE_API_KEY: string = import.meta.env.VITE_AZURE_API_KEY as string;
  const VITE_AZURE_AUTH_ENDPOINT: string = import.meta.env.VITE_AZURE_AUTH_ENDPOINT as string;
  const VITE_AZURE_TTS_ENDPOINT: string = import.meta.env.VITE_AZURE_TTS_ENDPOINT as string;
  const VITE_AZURE_ENDPOINT_WHISPER: string = import.meta.env.VITE_AZURE_ENDPOINT_WHISPER as string;
  const VITE_AZURE_KEY_WHISPER: string = import.meta.env.VITE_AZURE_KEY_WHISPER as string;

  async function transcribeAudio(audioFile: Blob): Promise<TranscriptionResponse> {
    try {
      const formData = new FormData();
      formData.append("file", audioFile, "audio.wav");

      const response = await axios.post<TranscriptionResponse>(VITE_AZURE_ENDPOINT_WHISPER, formData, {
        headers: {
          "api-key": VITE_AZURE_KEY_WHISPER,
          "Content-Type": "multipart/form-data",
        },
      });

      console.log("Transcription response:", response.data);
      return response.data;
    } catch (error: any) {
      console.error("Error transcribing audio:", error);
      if (error.response) {
        console.error("Response details:", error.response.data);
      }
      throw error;
    }
  }

  async function textToSpeech(text: string): Promise<void> {
    try {
      const tokenRes = await fetch(VITE_AZURE_AUTH_ENDPOINT, {
        method: "POST",
        headers: {
          "Ocp-Apim-Subscription-Key": VITE_AZURE_API_KEY,
          "Content-Length": "0",
        },
      });

      if (!tokenRes.ok) throw new Error("Failed to fetch Azure TTS token");

      const token: string = await tokenRes.text();

      const ssml: string = `
        <speak version='1.0' xml:lang='en-US'>
          <voice xml:lang='en-US' xml:gender='Male' name='en-US-NovaTurboMultilingualNeural'>
            ${text}
          </voice>
        </speak>`;

      const ttsRes = await fetch(VITE_AZURE_TTS_ENDPOINT, {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
          "Content-Type": "application/ssml+xml",
          "X-Microsoft-OutputFormat": "audio-16khz-32kbitrate-mono-mp3",
        },
        body: ssml,
      });

      if (!ttsRes.ok) throw new Error("Failed to convert text to speech");

      const audioBuffer: ArrayBuffer = await ttsRes.arrayBuffer();
      const audioBlob = new Blob([audioBuffer], { type: "audio/mp3" });
      const audioUrl: string = URL.createObjectURL(audioBlob);
      const audio = new Audio(audioUrl);
      audio.play();
    } catch (error) {
      console.error("TTS Error:", error);
    }
  }

  // Optional helper: converts markdown content to plain text.
  function markdownToPlainText(markdown: string): string {
    const html = marked(markdown);
    return htmlToText(html, { wordwrap: false, preserveNewlines: true });
  }

  return {
    transcribeAudio,
    textToSpeech,
    markdownToPlainText,
  };
}
