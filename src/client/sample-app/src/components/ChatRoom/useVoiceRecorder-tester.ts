import { useState, useRef, useEffect } from "react";

type UseVoiceRecordSettings = {
  onAudioReady: (blob: Blob) => void;
  silenceThreshold?: number; 
};

export const useVoiceRecorder = ({
  onAudioReady,
  silenceThreshold = 2000,
}: UseVoiceRecordSettings) => {
  const [isRecording, setIsRecording] = useState(false);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const audioChunksRef = useRef<Blob[]>([]);
  const silenceTimer = useRef<number | null>(null);
  const audioContextRef = useRef<AudioContext | null>(null);

  const frameBufferSize = 30;
  const frameBuffer = useRef<boolean[]>(Array(frameBufferSize).fill(false));


  

  useEffect(() => {
    const startRecording = async () => {
      console.log("[useVoiceRecorder] Hook mounted");

      try {
        console.log("[useVoiceRecorder] Attempting to access microphone...");
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        const mediaRecorder = new MediaRecorder(stream);
        mediaRecorderRef.current = mediaRecorder;

        mediaRecorder.ondataavailable = (event) => {
          if (event.data.size > 0) {
            audioChunksRef.current.push(event.data);
            console.log(`Chunk added: ${event.data.size} bytes`);
          }
        };

        mediaRecorder.onstop = () => {
          if (audioChunksRef.current.length === 0) {
            console.warn("No valid audio recorded. Waiting for a valid chunk...");
            return;
          }

          const audioBlob = new Blob(audioChunksRef.current, { type: "audio/wav" });

          if (audioBlob.size === 0) {
            console.warn("Audio Blob is empty. Skipping upload.");
            return;
          }

          console.log(`Merging ${audioChunksRef.current.length} chunks`);
          console.log(`Final file size: ${audioBlob.size} bytes`);
          onAudioReady(audioBlob);
         
          audioChunksRef.current = [];

          // Automatically restart after silence-based stop
          if (mediaRecorderRef.current?.state === "inactive") {
            mediaRecorderRef.current.start();
            setIsRecording(true);
            console.log("Restarted recording...");
          }
        };

        mediaRecorder.start();
        setIsRecording(true);
        console.log("Recording started...");

        // Silence detection setup
        const audioContext = new AudioContext();
        const analyser = audioContext.createAnalyser();
        analyser.fftSize = 1024;

        const microphone = audioContext.createMediaStreamSource(stream);
        microphone.connect(analyser);
        audioContextRef.current = audioContext;

        const detectSilence = () => {
          const buffer = new Uint8Array(analyser.frequencyBinCount);
          analyser.getByteFrequencyData(buffer);

          const avgVolume = buffer.reduce((sum, val) => sum + val, 0) / buffer.length;
          const currentFrameSilent = avgVolume < 10;

          frameBuffer.current.shift();
          frameBuffer.current.push(currentFrameSilent);

          const silentFrames = frameBuffer.current.filter(Boolean).length;
          const percentSilent = silentFrames / frameBufferSize;

          if (percentSilent > 0.8) {
            if (!silenceTimer.current) {
              silenceTimer.current = window.setTimeout(() => {
                console.log("Silence detected. Stopping recording...");

                const mr = mediaRecorderRef.current;
                if (mr && mr.state === "recording") {
                  try {
                    mr.requestData();
                    setTimeout(() => {
                      if (mr.state === "recording") {
                        mr.stop();
                        setIsRecording(false);
                      }
                    }, 100);
                  } catch (err) {
                    console.warn("Failed to request data or stop recorder:", err);
                  }
                }
              }, silenceThreshold);
            }
          } else {
            if (silenceTimer.current) {
              clearTimeout(silenceTimer.current);
              silenceTimer.current = null;
            }
          }

          requestAnimationFrame(detectSilence);
        };

        detectSilence();
      } catch (err) {
        console.error("Microphone access failed:", err);
      }
    };

    startRecording();

    return () => {
      if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
        mediaRecorderRef.current.stop();
      }
      if (silenceTimer.current) {
        clearTimeout(silenceTimer.current);
      }
      if (audioContextRef.current && audioContextRef.current.state !== "closed") {
        audioContextRef.current.close();
      }
    };
  }, [onAudioReady, silenceThreshold]);

  return { isRecording };
};
