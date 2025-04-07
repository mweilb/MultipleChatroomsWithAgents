import React, {
  useState,
  useCallback,
  useRef,
  useEffect,
  useMemo,
  JSX,
} from "react";
import "./ChatRoom.scss";
import "./ChatRoom.css";
import { Button } from "primereact/button";
import { InputTextarea } from "primereact/inputtextarea";
import { Steps } from "primereact/steps";
import { useNavigate } from 'react-router-dom';


import { useWebSocketContext } from "./contexts/webSocketContext";
import { WebSocketBaseMessage } from "./models/WebSocketBaseMessage";
import ReactMarkdown from "react-markdown";
import { WebSocketReplyChatRoomMessage } from "./models/WebSocketReplyChatRoomMessages";
import { useAppStateContext } from "./context-app/AppStateContext";
import VoiceControl from "./context-app/voiceControl";
import { AudioPlayer } from "./AudioPlayer";
import { WebSocketAudioMessage } from "./models/WebSocketVoiceMessage";

const ChatRoomSave = (): JSX.Element => {
  // Context from your existing app state.
  const { activeChatRoomName, activeChannel, activeChatSubRoomName, availableRoomNames, requestRoomChange, getMessagesForChannel, setDidRoomChange, nextRoom } = useAppStateContext();
  const { sendMessage, setAudioMessageListener } = useWebSocketContext();
  
  // Get messages filtered by active room and subroom.
  const messages: WebSocketReplyChatRoomMessage[] = getMessagesForChannel();

  // Local state for message input and help modal.
  const [message, setMessage] = useState<string>("");
  const [translate, setTranslate] = useState<string>("");
  const [isHelp, setIsHelp] = useState<boolean>(false);

  // Local ref for auto scrolling to the last message.
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Toggle help modal.
  const toggleHelp = (): void => setIsHelp((prev) => !prev);
  const audioPlayerRef = useRef(new AudioPlayer());

  useEffect(() => {
    setAudioMessageListener((msg: WebSocketAudioMessage) => {
      if (msg.SubAction === "chunk") {
        audioPlayerRef.current.playChunk(msg);
      } else if (msg.SubAction === "done") {
        console.log("Audio stream ended.");
      }
    });
  }, [setAudioMessageListener]);

  // Handle message sending.
  const handleClickSendMessage = useCallback(
    (textOverride?: string): void => {
      const contentToSend: string = textOverride ?? message;
      if (contentToSend.trim() !== "") {
        const socketMessage: WebSocketBaseMessage = {
          UserId: "12345",
          TransactionId: crypto.randomUUID(),
          Action: activeChatRoomName,
          SubAction: "ask",
          RoomName: activeChatRoomName,
          SubRoomName: activeChatSubRoomName,
          Content: contentToSend,
        };

        sendMessage(socketMessage);
        setMessage("");
      }
    },
    [message, sendMessage, activeChatRoomName, activeChatSubRoomName]
  );

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>): void => {
    if (e.key === "Enter") {
      e.preventDefault();
      handleClickSendMessage();
    }  };

  const handleTranscriptChange = (transcript: string) => {
    setTranslate(transcript);
    if (transcript.includes("send")) {
      handleClickSendMessage(translate);
      setTranslate("");
    }  
  }

  const navigate = useNavigate();
  const goBack = () => {
    navigate('/');
  }

  const roomChange = (change:boolean) => {
    if(change) {
      setDidRoomChange('changeRoom');
      console.log("Change room pressed")
    } else {
      setDidRoomChange('noChange');
      console.log("No room change pressed")
    }
  }

  // Auto-scroll to bottom when new messages arrive.
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const steps = [...new Set(availableRoomNames)];
  const stepsItems = useMemo(() => {
    return steps.map(stage => ({
      label: stage,
    }));
  }, [availableRoomNames]);

  return (
    <div id="chat-container" className="chat-container">
      <div className="header">
        <div className="header-top">
          <span className="pi pi-arrow-left" onClick={goBack}/>
          <h1 className="header-title"> {activeChatRoomName} </h1>
          <Button icon="pi pi-info-circle" className="help-button" onClick={toggleHelp} />
        </div>
        <Steps
          model={stepsItems.map((item, index) => ({
            ...item,
            className: [
              `step-${index}`,
              index < activeChannel ? "p-complete" : "",
              index === activeChannel ? "p-highlight" : "",
            ].join(" "),
          }))}
          activeIndex={activeChannel}
          className={`mt-4 custom-steps ${stepsItems.length === 1 ? "single-step" : ""}`}
        />
      </div>

      {isHelp && (
        <div className="help-container">
          <div className="modal">
            <Button icon="pi pi-times" className="modal-button" onClick={toggleHelp} />
            <p className="modal-text">
              Transferring you to a human assistant. Please hold on while I connect you...
            </p>
          </div>
        </div>
      )}

      <div className="chat-messages">
        {messages.map((msg) => {
          const messageClass: string = msg.AgentName.toLowerCase()
            .replace(/[^a-z0-9]+/g, "-")
            .replace(/^-+|-+$/g, "");
          const isUserMessage: boolean = messageClass === "user";

          return (
            <div key={msg.TransactionId} className={`message-wrapper ${messageClass}`}>
              <div className={`message-container ${messageClass}`}>
                <div className={`message-info ${messageClass}`}>
                  <div className="agent-name">{msg.AgentName}</div>
                  {/* <div className="agent-emoji">{msg.Emoji}</div> */}
                </div>
                <div
                  className={`message-content ${messageClass}`}
                  style={{
                    backgroundColor: isUserMessage
                    ? "#1b85b8"
                    : "white",
                    color: isUserMessage ? "" : "#000",
                  }}
                >
                  <ReactMarkdown className="markdown-content">{msg.Content}</ReactMarkdown>
                </div>
              </div>
            </div>
          );
        })}
        {/* Dummy div to scroll into view */}
        <div ref={messagesEndRef} />
      </div>

      {requestRoomChange && (
        <div className="change-room-container">
          <p>Would you like to move to the next room:<br />{nextRoom}?</p>
          <div className="change-button-container">
            <Button className="change-button yes" onClick={() => roomChange(true)}>Yes</Button>
            <Button className="change-button no" onClick={() => roomChange(false)}>No</Button>
          </div>
        </div>
      )}

      <div className="chat-input">
        <InputTextarea
            value={translate.trim() !== "" ? translate : message}
            disabled={translate.trim() !== ""}
            onKeyDown={handleKeyDown}
            placeholder="Type a message..."
            rows={1}
            autoResize
            onChange={(e) => {
              // When the user manually types, we clear the translate value if present.
              if (translate.trim() !== "") {
                setTranslate("");
              }
              setMessage(e.target.value);
            }}
            className="chat-input-field"
          />
         <VoiceControl onTranscriptChange={handleTranscriptChange} />
        <div className="input-icons">
          <Button className="send-btn" label="Send" onClick={() => handleClickSendMessage()} />
        </div>
      </div>
    </div>
  );
};

export default ChatRoomSave;
