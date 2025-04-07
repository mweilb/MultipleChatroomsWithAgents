import React, { useEffect } from 'react';
import SpeechRecognition, { useSpeechRecognition } from 'react-speech-recognition';
import './VoiceControl.css';
import { PiMicrophoneFill } from "react-icons/pi";
import { PiMicrophoneSlashFill } from "react-icons/pi";


interface VoiceControlProps {
  onTranscriptChange?: (transcript: string) => void;
}

const VoiceControl: React.FC<VoiceControlProps> = ({ onTranscriptChange }) => {
  const { transcript, listening, resetTranscript, browserSupportsSpeechRecognition } = useSpeechRecognition();

  // Clear transcript on mount.
  useEffect(() => {
    resetTranscript();
  }, [resetTranscript]);

  // Notify parent on transcript update.
  useEffect(() => {
    if (onTranscriptChange) {
      onTranscriptChange(transcript);
    }
  }, [transcript, onTranscriptChange]);

  if (!browserSupportsSpeechRecognition) {
    return <span className="vc-error">Your browser does not support Speech Recognition.</span>;
  }

  // Toggle microphone on and off.
  const toggleListening = () => {
    if (listening) {
      SpeechRecognition.stopListening();
      if (onTranscriptChange) {
        onTranscriptChange("");
      }
    } else {
      resetTranscript();
      SpeechRecognition.startListening({ continuous: true });
    }
  };

  return (
    <div className="vc-container" onClick={toggleListening}>
      
        {listening 
        ? <PiMicrophoneFill className='mic-icon'/>
        : <PiMicrophoneSlashFill className='mic-icon'/>
        }
      
    </div>
  );
};

export default VoiceControl;
