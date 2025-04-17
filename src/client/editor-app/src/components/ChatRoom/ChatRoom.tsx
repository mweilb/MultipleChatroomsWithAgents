import React, { useState, useEffect, useRef, useCallback } from 'react';
import MessageList from './MessageList';  // Component that handles rendering messages
import ChatInput from './ChatInput';      // Component that handles input field and send button
import { useWebSocketContext, WebSocketBaseMessage, WebSocketAudioMessage, WebSocketNewRoomMessage } from 'shared'; // Custom hook for WebSocket context
import { AudioPlayer } from "../../audio/AudioPlayer";
import './ChatRoom.css';

interface ChatRoomProps {
  /** The type of the chat, used to filter and display relevant messages */
  chatType: string;
  
  /** The title to display for the chat room */
  title: string;
  
  /** The unique user identifier */
  userId: string;
}

const ChatRoom: React.FC<ChatRoomProps> = ({ chatType, title, userId }) => {
  const { getMessages, sendMessage, setAudioMessageListener, resetChat, requestRoomChange, rooms, addOrUpdateMessage } = useWebSocketContext();
  const [input, setInput] = useState('');
  const [showRationales, setShowRationales] = useState(false);
  const [selectedRoom, setSelectedRoom] = useState(chatType);

  useEffect(() => {
    setSelectedRoom(chatType);
  }, [chatType]);

  const toggleRationales = () => {
    setShowRationales(prev => !prev);
  };

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const audioPlayerRef = useRef(new AudioPlayer());
  const messages = getMessages(chatType);

  useEffect(() => {
    // Always scroll to the bottom when messages change
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  useEffect(() => {
    setAudioMessageListener((msg: WebSocketAudioMessage) => {
      if (msg.SubAction === "chunk") {
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
  }, [input, userId, chatType, sendMessage]);

  const handleKeyDown = useCallback((e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  }, [handleSend]);

  // Function to handle scrolling the messages container to the top
  const handleReset = () => {
    resetChat(chatType);
  };

  return (
    <div className="chat-room">
      <div className="chat-room-header">
        <div className="chat-room-title">{title}</div>
        <div className="chat-room-controls">
          <select
            value={selectedRoom}
            onChange={e => {
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
                  Emoji: '🤓',
                  To: newRoom,
                  From: chatType,
                  SubAction: "change-room-yield",
                  Content: `Do you want to change the room to "${newRoom}"?`,
                  Orchestrator: ''
                };


                // Prompt user for confirmation before changing room
                addOrUpdateMessage(roomMessage);
              }
            }}
            className="toggle-button select-room">
            {(rooms?.find(r => r.Name === chatType)?.Rooms || []).map((subroom, idx) => (
              <option key={subroom.Name || idx} value={subroom.Name}>
                {subroom.Emoji ? subroom.Emoji : ''} {subroom.Name} 
              </option>
            ))}
          </select>
          <button
            onClick={toggleRationales}
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
        {/* Settings panel removed as features are now in the toolbar */}
        <div className="chat-interface">
          <div className="messages-container" ref={messagesContainerRef}>
            <MessageList 
              messages={messages} 
              chatType={chatType} 
              showRationales={showRationales}
              onRoomChange={(message) => {
                // Extract the new room name from RoomName or fallback to Content
                const newRoom = (message as WebSocketNewRoomMessage);
               
                if (newRoom) {
                  setSelectedRoom(newRoom.To);
                }
              }}
              onRoomChangeYieldAnswer={(message, answer) => {
                // You can handle the answer here, e.g., send to server or update state
                var roomMessage = message as WebSocketNewRoomMessage;
                if (roomMessage != null){
                  if (answer == "Yes"){
                    requestRoomChange(userId, message.Action, roomMessage.To);
                  }else{
                    requestRoomChange(userId,message.Action, roomMessage.From);
                  }
                }

              }}
            />
            <div ref={messagesEndRef} />
          </div>
          <div className="chat-input"> 
            <ChatInput
              input={input}
              onInputChange={setInput}
              onSend={handleSend}
              onKeyDown={handleKeyDown}
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default ChatRoom;
