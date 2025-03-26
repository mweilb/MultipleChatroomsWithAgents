import React, { useState, useEffect } from 'react';
 
import ChatInput from '../ChatRoom/ChatInput';
import './Librarian.css';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import { CodeBlock } from '../ChatRoom/CodeBlock';
import { useWebSocketContext,WebSocektLibrainDocRef } from 'shared';
import { ReferenceItem } from './ReferenceItem';

interface ListWithLibrarianControlProps {
  roomName: string;
  librarianName: string;
}

const STORAGE_KEY = 'listWithLibrarianControl_showReferencesMap';

const ListWithLibrarianControl: React.FC<ListWithLibrarianControlProps> = ({ roomName, librarianName }) => {
  // Local state for the message input.
  const [messageInput, setMessageInput] = useState<string>('');
  
  // State to track which message's references block is visible.
  const [showReferencesMap, setShowReferencesMap] = useState<{ [transactionId: string]: boolean }>(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : {};
  });

  // Persist showReferencesMap to localStorage when it changes.
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(showReferencesMap));
  }, [showReferencesMap]);

  // Get the sendList and getLibrarianListMessages functions from context.
  const { sendList, getLibrarianListMessages } = useWebSocketContext();

  // Retrieve conversation messages from the store in their natural order.
  const conversation = getLibrarianListMessages(roomName, librarianName);

  // Handler for sending a message.
  const handleSendMessage = () => {
    if (messageInput.trim() === '') return; // Prevent sending empty messages.
    
    // Send the payload.
    sendList(roomName, librarianName, messageInput);
    setMessageInput(''); // Clear the text editor after sending.
  };

  // Handler for keydown events to send message on Enter (without Shift).
  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  return (
    <div className="converse-control-container">
      {/* Header displaying agent@room */}
      <div className="converse-header">
        <div>{librarianName}@{roomName}</div>
      </div>

      {/* Conversation History Area */}
      <div className="conversation-history">
        {conversation.length === 0 ? (
          <p className="placeholder-text">No conversations yet. Start chatting!</p>
        ) : (
          conversation.map((msg) => (
            <div key={msg.TransactionId} className="conversation-message">
              <div className="message-question">Question: {msg.Question}</div>
              <ReactMarkdown
               
                components={{ code: CodeBlock }}
                remarkPlugins={[remarkGfm]}
                rehypePlugins={[rehypeHighlight]}
              >
                {msg.Content}
              </ReactMarkdown>
              {msg.References && msg.References.length > 0 && (
                <>
                  <button 
                    onClick={() => 
                      setShowReferencesMap(prev => ({
                        ...prev,
                        [msg.TransactionId]: !prev[msg.TransactionId]
                      }))
                    }
                  >
                    {showReferencesMap[msg.TransactionId] ? 'Hide References' : 'Show References'}
                  </button>
                  {showReferencesMap[msg.TransactionId] && (
                    <div className="message-references">
                      <h4>References:</h4>
                      {msg.References.map((ref: WebSocektLibrainDocRef, index: number) => (
                        <ReferenceItem key={index} reference={ref} />
                      ))}
                    </div>
                  )}
                </>
              )}
            </div>
          ))
        )}
      </div>

      {/* ChatInput component */}
      <ChatInput 
        input={messageInput}
        onInputChange={setMessageInput}
        onSend={handleSendMessage}
        onKeyDown={handleKeyDown}
      />
    </div>
  );
};

export default ListWithLibrarianControl;
