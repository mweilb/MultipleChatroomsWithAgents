import React, { useState, useEffect, useRef, useCallback } from 'react';
import MessageList from './MessageList';
import ChatInput from './ChatInput';
import { useWebSocketContext, WebSocketBaseMessage, WebSocketAudioMessage, WebSocketNewRoomMessage } from 'shared';
import { AudioPlayer } from "../../audio/AudioPlayer";
import './ChatRoom.css';

interface ChatRoomProps {
  chatType: string;
  title: string;
  userId: string;
}

const ChatRoom: React.FC<ChatRoomProps> = ({ chatType, title, userId }) => {
  const {
    getMessages,
    sendMessage,
    setAudioMessageListener,
    resetChat,
    requestRoomChange,
    rooms,
    addOrUpdateMessage,
  } = useWebSocketContext();

  const [input, setInput] = useState('');
  const [showRationales, setShowRationales] = useState(false);
  const [selectedRoom, setSelectedRoom] = useState(chatType);
 

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const audioPlayerRef = useRef(new AudioPlayer());
  const messages = getMessages(chatType);

  useEffect(() => {
    setSelectedRoom(chatType);
  }, [chatType]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    
  }, [messages]);

  useEffect(() => {
    setAudioMessageListener((msg: WebSocketAudioMessage) => {
      if (msg.SubAction === "reply") {
        audioPlayerRef.current.playChunk(msg);
      } else if (msg.SubAction === "done") {
        console.log("Audio stream ended.");
      }
    });
  }, [setAudioMessageListener]);

  const handleSend = useCallback(() => {
    if (!input.trim()) return;
    const message: WebSocketBaseMessage = {
      UserId: userId,
      TransactionId: crypto.randomUUID(),
      Action: chatType,
      SubAction: 'ask',
      Content: input,
      Mode: 'App',
    };
    sendMessage(message, chatType, selectedRoom);
    setInput('');
  }, [input, userId, chatType, sendMessage, selectedRoom]);

  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        handleSend();
      }
    },
    [handleSend]
  );

  const handleReset = useCallback(() => {
    resetChat(chatType);
  }, [resetChat, chatType]);

  const handleToggleRationales = useCallback(() => {
    setShowRationales((prev) => !prev);
  }, []);

  const handleRoomChangeSelect = useCallback(
    (e: React.ChangeEvent<HTMLSelectElement>) => {
      const newRoom = e.target.value;
      if (newRoom !== chatType) {
        setSelectedRoom(newRoom);

        const roomMessage: WebSocketNewRoomMessage = {
          UserId: userId,
          TransactionId: crypto.randomUUID(),
          Action: chatType,
          RoomName: newRoom,
          Mode: "App",
          AgentName: 'User',
          DisplayName: 'User',
          Emoji: '🤓',
          To: newRoom,
          From: chatType,
          SubAction: "change-room-yield",
          Content: `Do you want to change the room to "${newRoom}"?`,
          Orchestrator: ''
        };

        const existing = messages.find(
          (msg) =>
            msg.SubAction === "change-room-yield" &&
            (msg as WebSocketNewRoomMessage).To === newRoom &&
            (msg as WebSocketNewRoomMessage).From === chatType
        );
        if (!existing) {
          addOrUpdateMessage(roomMessage);
 
        }
      }
    },
    [chatType, userId, messages, addOrUpdateMessage]
  );

  const handleRoomChange = useCallback(
    (message: any) => {
      // Accept any type, but only update if To exists
      if (message && message.To) {
        setSelectedRoom(message.To);
      }
    },
    []
  );

  const handleRoomChangeYieldAnswer = useCallback(
    (message: any, answer: string) => {
 
      if (message && message.To && message.From) {
        if (answer === "Yes") {
          requestRoomChange(userId, message.Action, message.To);
        } else {
          requestRoomChange(userId, message.Action, message.From);
        }
      }
    },
    [requestRoomChange, userId]
  );

  return (
    <div className="chat-room">
      <div className="chat-room-header">
        <div className="chat-room-title">{title}</div>
        <div className="chat-room-controls">
          <select
            value={selectedRoom}
            onChange={handleRoomChangeSelect}
            className="toggle-button select-room"
            aria-label="Select chat room"
          >
            {(rooms?.find(r => r.Name === chatType)?.Rooms || []).map((subroom, idx) => (
              <option key={subroom.Name || idx} value={subroom.Name}>
                {subroom.Emoji ? subroom.Emoji : ''} {subroom.DisplayName}
              </option>
            ))}
          </select>
          <button
            onClick={handleToggleRationales}
            className={`toggle-button rationales ${showRationales ? 'active' : ''}`}
          >
            {showRationales ? 'Hide Rationales' : 'Show Rationales'}
          </button>
          <button
            onClick={handleReset}
            className="reset-to-top-button"
            disabled={messages.length === 0}
          >
            Reset Chat
          </button>
        </div>
      </div>

      <div className="chat-room-body">
        <div className="chat-interface">
          <div className="messages-container" ref={messagesContainerRef}>
            <MessageList
              messages={messages}
              chatType={chatType}
              showRationales={showRationales}
              onRoomChange={handleRoomChange}
              onRoomChangeYieldAnswer={handleRoomChangeYieldAnswer}
            />
            <div ref={messagesEndRef} />
          </div>
          <div className="chat-input">
            <ChatInput
              input={input}
              onInputChange={setInput}
              onSend={handleSend}
              onKeyDown={handleKeyDown}
              actionKey={chatType}
             />
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatRoom;
