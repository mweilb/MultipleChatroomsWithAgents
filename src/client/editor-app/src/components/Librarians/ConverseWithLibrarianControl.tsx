import React, { useState } from 'react';
import { useWebSocketContext } from 'shared';
import ChatInput from '../ChatRoom/ChatInput';
import './Librarian.css';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import { CodeBlock } from '../ChatRoom/CodeBlock';

interface ConverseWithLibrarianControlProps {
  roomName: string;
  librarianName: string;
}

interface ConversationMessageProps {
  msg: any; // Replace 'any' with your message type if available.
}

const ConversationMessage: React.FC<ConversationMessageProps> = ({ msg }) => {
  // Answer (Content) is shown by default; Thinking is hidden by default.
  const [showAnswer, setShowAnswer] = useState(true);
  const [showThinking, setShowThinking] = useState(false);

  return (
    <div className="conversation-message">
      <div className="message-question">Question: {msg.Question}</div>
      
      <div className="message-section">
        <div className="message-label">Answer:</div>
        <button onClick={() => setShowAnswer(prev => !prev)}>
          {showAnswer ? 'Hide Answer' : 'Show Answer'}
        </button>
        {showAnswer && (
          <ReactMarkdown
        
            components={{ code: CodeBlock }}
            remarkPlugins={[remarkGfm]}
            rehypePlugins={[rehypeHighlight]}
          >
            {msg.Content}
          </ReactMarkdown>
        )}
      </div>

      {msg.Thinking && (
        <div className="message-section">
          <div className="message-label">Thinking:</div>
          <button onClick={() => setShowThinking(prev => !prev)}>
            {showThinking ? 'Hide Thinking' : 'Show Thinking'}
          </button>
          {showThinking && (
            <ReactMarkdown
            
              components={{ code: CodeBlock }}
              remarkPlugins={[remarkGfm]}
              rehypePlugins={[rehypeHighlight]}
            >
              {msg.Thinking}
            </ReactMarkdown>
          )}
        </div>
      )}
    </div>
  );
};

const ConverseWithLibrarianControl: React.FC<ConverseWithLibrarianControlProps> = ({ roomName, librarianName }) => {
  // Local state for the message input.
  const [messageInput, setMessageInput] = useState<string>('');

  // Get the sendConverse and getLibrarianConverseMessages functions from context.
  const { sendConverse, getLibrarianConverseMessages } = useWebSocketContext();

  // Retrieve conversation messages from the store in their natural order.
  const conversation = getLibrarianConverseMessages(roomName, librarianName);

  // Handler for sending a message.
  const handleSendMessage = () => {
    if (messageInput.trim() === '') return; // Prevent sending empty messages.
    sendConverse(roomName, librarianName, messageInput);
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
            <ConversationMessage key={msg.TransactionId} msg={msg} />
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

export default ConverseWithLibrarianControl;
