import { WebSocketAudioMessage } from "shared";

export class AudioPlayer {
  // Queue to hold object URLs for audio chunks.
  private queue: string[] = [];
  private isPlaying = false;
  private isStreamEnded = false;
  // Set a maximum queue size to prevent runaway memory usage.
  private maxQueueSize = 5000;

  constructor() {}

  // Convert a Base64 string to a Uint8Array.
  private base64ToUint8Array(base64: string): Uint8Array {
    const binaryString = window.atob(base64);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return bytes;
  }

  /**
   * Create a WAV Blob from raw PCM data.
   *
   * @param audioBuffer - The raw PCM data as a Uint8Array.
   * @param sampleRate - The sample rate (Hz) of the audio.
   * @param channels - Number of audio channels (default is 1).
   * @param bitsPerSample - Bits per sample (default is 16).
   * @returns A Blob containing the complete WAV file.
   */
  private createWavBlob(
    audioBuffer: Uint8Array,
    sampleRate: number,
    channels: number = 1,
    bitsPerSample: number = 16
  ): Blob {
    // Create an ArrayBuffer for the 44-byte WAV header.
    const header = new ArrayBuffer(44);
    // DataView allows us to set values in the header.
    const view = new DataView(header);

    // Helper function to write an ASCII string into the DataView.
    function writeString(view: DataView, offset: number, str: string) {
      for (let i = 0; i < str.length; i++) {
        view.setUint8(offset + i, str.charCodeAt(i));
      }
    }

    // --- Begin WAV header creation ---
    // "RIFF" chunk descriptor.
    writeString(view, 0, "RIFF");
    // File size minus 8 bytes for "RIFF" and file size field.
    view.setUint32(4, 36 + audioBuffer.length, true);
    // "WAVE" identifier.
    writeString(view, 8, "WAVE");

    // "fmt " subchunk: Contains audio format information.
    writeString(view, 12, "fmt ");
    // Subchunk size: 16 for PCM.
    view.setUint32(16, 16, true);
    // Audio format: 1 indicates PCM (uncompressed).
    view.setUint16(20, 1, true);
    // Number of channels.
    view.setUint16(22, channels, true);
    // Sample rate (Hz).
    view.setUint32(24, sampleRate, true);
    // Byte rate = SampleRate * NumChannels * BitsPerSample/8.
    view.setUint32(28, sampleRate * channels * bitsPerSample / 8, true);
    // Block align = NumChannels * BitsPerSample/8.
    view.setUint16(32, channels * bitsPerSample / 8, true);
    // Bits per sample.
    view.setUint16(34, bitsPerSample, true);

    // "data" subchunk: Contains the audio data.
    writeString(view, 36, "data");
    // Data chunk length: number of bytes in the audio data.
    view.setUint32(40, audioBuffer.length, true);
    // --- End WAV header creation ---

    // Combine header and audio data into a single Uint8Array.
    const wavBuffer = new Uint8Array(header.byteLength + audioBuffer.length);
    wavBuffer.set(new Uint8Array(header), 0); // Set header.
    wavBuffer.set(audioBuffer, header.byteLength); // Append audio data.

    // Return a Blob containing the complete WAV file data.
    return new Blob([wavBuffer], { type: "audio/wav" });
  }

  // Call this method when the stream is finished (if applicable).
  public endStream(): void {
    this.isStreamEnded = true;
  }

  // Queue and play a single audio chunk.
  public playChunk(audioMessage: WebSocketAudioMessage): void {
    if (!audioMessage.AudioData) {
      console.warn("Empty audio data received.");
      return;
    }
    if (this.isStreamEnded) {
      console.warn("Stream has ended. Ignoring new audio chunk.");
      return;
    }

    const audioBytes = this.base64ToUint8Array(audioMessage.AudioData);
    let blob: Blob;

    // Wrap PCM data in a WAV container; if format is MP3, use that directly.
    if (audioMessage.AudioFormat.toLowerCase() === "pcm") {
      blob = this.createWavBlob(audioBytes, audioMessage.SampleRate, 1, 16);
    } else if (audioMessage.AudioFormat.toLowerCase() === "mp3") {
      blob = new Blob([audioBytes], { type: "audio/mp3" });
    } else {
      console.warn("Unsupported audio format:", audioMessage.AudioFormat);
      return;
    }

    const audioUrl = URL.createObjectURL(blob);

    // Enforce a maximum queue size.
    if (this.queue.length >= this.maxQueueSize) {
      console.warn("Audio queue full. Dropping oldest chunk.");
      const droppedUrl = this.queue.shift();
      if (droppedUrl) {
        URL.revokeObjectURL(droppedUrl);
      }
    }
    this.queue.push(audioUrl);

    // If no audio is playing, start playback.
    if (!this.isPlaying) {
      this.playNextChunk();
    }
  }

  // Plays the next chunk in the queue.
  private playNextChunk(): void {
    if (this.queue.length === 0) {
      this.isPlaying = false;
      return;
    }
    this.isPlaying = true;
    const url = this.queue.shift();
    if (!url) {
      this.isPlaying = false;
      return;
    }

    const audio = new Audio(url);
    const cleanup = () => {
      // Revoke the object URL to free up memory.
      URL.revokeObjectURL(url);
      // Continue with the next chunk.
      this.playNextChunk();
    };

    // When playback ends, clean up and move to the next chunk.
    audio.onended = cleanup;
    // On error, log and clean up.
    audio.onerror = (e) => {
      console.error("Error playing audio chunk:", e);
      cleanup();
    };

    try {
      audio.play().catch((err) => {
        console.error("Audio play promise rejected:", err);
        cleanup();
      });
    } catch (err) {
      console.error("Error during audio.play:", err);
      cleanup();
    }
  }
}
