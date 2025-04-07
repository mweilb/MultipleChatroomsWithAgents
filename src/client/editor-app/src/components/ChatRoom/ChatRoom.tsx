import React, { useState, useEffect, useRef, useCallback } from 'react';
import MessageList from './MessageList';  // Component that handles rendering messages
import ChatInput from './ChatInput';      // Component that handles input field and send button
import { useWebSocketContext, WebSocketBaseMessage, WebSocketAudioMessage } from 'shared'; // Custom hook for WebSocket context
import { AudioPlayer } from "../../audio/AudioPlayer";
import './ChatRoom.css';
import ChatSettingComponent from './ChatSetttingComponent';

interface ChatRoomProps {
  /** The type of the chat, used to filter and display relevant messages */
  chatType: string;
  
  /** The title to display for the chat room */
  title: string;
  
  /** The unique user identifier */
  userId: string;
}

const ChatRoom: React.FC<ChatRoomProps> = ({ chatType, title, userId }) => {
  const { getMessages, sendMessage, setAudioMessageListener, resetChat, } = useWebSocketContext();
  const [input, setInput] = useState('');
  
  // State to toggle display of rationales and settings
  const [showRationales, setShowRationales] = useState(false);
  const [showSettings, setShowSettings] = useState(true);

  // Toggle Rationales: if showing rationales, hide settings.
  const toggleRationales = () => {
    setShowRationales(prev => {
      const newVal = !prev;
      return newVal;
    });
  };

  // Toggle Settings: if showing settings, hide rationales.
  const toggleSettings = () => {
    setShowSettings(prev => {
      const newVal = !prev;
      return newVal;
    });
  };

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const audioPlayerRef = useRef(new AudioPlayer());
  const messages = getMessages(chatType);

  useEffect(() => {
    // Optionally, scroll to the bottom when new messages arrive
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
      SubRoomName: "",
      RoomName: "",
      Mode: 'App',  
    };
    
    sendMessage(message);
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
          <button
            onClick={toggleRationales}
            className={`toggle-button ${showRationales ? 'active' : ''}`}
          >
            {showRationales ? 'Hide Rationales' : 'Show Rationales'}
          </button>
          <button
            onClick={toggleSettings}
            className={`toggle-button ${showSettings ? 'active' : ''}`}
          >
            {showSettings ? 'Hide Settings' : 'Show Settings'}
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
        {showSettings && (
          <div className="chat-room-settings">
            <ChatSettingComponent currentRoomName={chatType} />
          </div>
        )}
        <div className="chat-interface">
          <div className="messages-container" ref={messagesContainerRef}>
            <MessageList 
              messages={messages} 
              chatType={chatType} 
              showRationales={showRationales}  // Pass down the flag to MessageList
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
