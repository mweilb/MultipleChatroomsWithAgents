import React, { useState, useEffect, useRef, useCallback } from 'react';
import MessageList from './MessageList';  // Component that handles rendering messages
import ChatInput from './ChatInput';      // Component that handles input field and send button
import { useWebSocketContext, WebSocketBaseMessage, WebSocketAudioMessage } from 'shared'; // Custom hook for WebSocket context
 
import { AudioPlayer } from "../../audio/AudioPlayer";

import './ChatRoom.css';
import Filters from '../Layout/Navigation/Filters';
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
  const { getMessages, sendMessage, setAudioMessageListener } = useWebSocketContext();
  const [input, setInput] = useState('');
  
  // States to control visibility of Filters and Settings
  const [showFilters, setShowFilters] = useState(false);
  const [showSettings, setShowSettings] = useState(false);

  // Toggle Filters: if showing filters, hide settings.
  const toggleFilters = () => {
    setShowFilters(prev => {
      const newVal = !prev;
      if (newVal) setShowSettings(false);
      return newVal;
    });
  };

  // Toggle Settings: if showing settings, hide filters.
  const toggleSettings = () => {
    setShowSettings(prev => {
      const newVal = !prev;
      if (newVal) setShowFilters(false);
      return newVal;
    });
  };

  // Hide both panels.
  const hideAll = () => {
    setShowFilters(false);
    setShowSettings(false);
  };

  const messagesEndRef = useRef<HTMLDivElement>(null);
  const audioPlayerRef = useRef(new AudioPlayer());
  const messages = getMessages(chatType);

  useEffect(() => {
    //messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
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
      Hints: {}
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

  return (
    <div className="chat-room">
      <div className="chat-room-header">
        <div className="chat-room-title">{title}</div>
        <div className="chat-room-controls">
          <button
            onClick={toggleFilters}
            className={`toggle-button ${showFilters ? 'active' : ''}`}
          >
            {showFilters ? 'Hide Filters' : 'Show Filters'}
          </button>
          <button
            onClick={toggleSettings}
            className={`toggle-button ${showSettings ? 'active' : ''}`}
          >
            {showSettings ? 'Hide Settings' : 'Show Settings'}
          </button>
          <button onClick={hideAll} className="toggle-button">
            Hide All
          </button>
        </div>
      </div>

      <div className="chat-room-body">
        {showFilters && (
          <div className="chat-room-filters">
            <Filters />
          </div>
        )}
        {showSettings && (
          <div className="chat-room-settings">
           <ChatSettingComponent currentRoomName={chatType}/>
          </div>
        )}
        <div className="chat-interface">
          <div className="messages-container">
            <MessageList messages={messages} chatType={chatType} />
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
