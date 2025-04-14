import React, { memo } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import 'highlight.js/styles/github.css';
import './ChatRoom.css';

import { WebSocketReplyChatRoomMessage } from 'shared';
import { CodeBlock } from './CodeBlock';

interface MessageListProps {
  messages: WebSocketReplyChatRoomMessage[]; // Array of chat messages to be displayed
  chatType: string; // Type of chat for styling or categorization (currently unused)
  showRationales: boolean; // Flag to show/hide rationales (Editor messages)
  onRoomChangeYieldAnswer?: (message: WebSocketReplyChatRoomMessage, answer: string) => void;
  onRoomChange?: (message: WebSocketReplyChatRoomMessage) => void;
}

const MessageList: React.FC<MessageListProps> = ({ messages, showRationales, onRoomChangeYieldAnswer, onRoomChange }) => {
  return (
    <div className="message-list-container">
      <div className="messages">
        {messages.length === 0 ? (
          <p>No messages yet.</p>
        ) : (
          messages.map((message) => {
            // Determine the type of message
            const isQuestion = message.SubAction === 'ask';
            const isRoomChangeYield = message.SubAction === 'change-room-yield';
            const isRoomChange = message.SubAction === 'change-room';
            const isAppEditor = message.Mode === 'Editor';
            


            // Use showRationales prop to control the display of editor messages.
            if (isAppEditor && !showRationales) {
              return null;
            }

            
            return (
              <div
                key={message.TransactionId}
                className={`message${isAppEditor ? ' rationale' : ''}${isQuestion ? ' question' : ''}${isRoomChangeYield ? ' change-room-yield' : ''}`}
              >
                {/* Render the agent's name if the message is not a question and is not from an Editor */}
                {!isQuestion && !isAppEditor && !isRoomChange && !isRoomChangeYield && message.AgentName?.trim() && (
<div className="agent-info">
                    <div className="agent-name-box">
                      <span className="agent-name">{message.AgentName}</span>
                      <span className="agent-icon" style={{ marginLeft: '8px' }}>{message.Emoji}</span>
                    </div>
                  </div>
                )}
                {(isRoomChange || isRoomChangeYield) && (
                  <>
                    {onRoomChange && onRoomChange(message)}
                    <div className="agent-info room-change">
                      <div className="agent-name">{message.AgentName}</div>
                      <div className="agent-icon">{message.Emoji}</div>
                    </div>
                  </>
                )}
                <div className="message-content">
                  {isRoomChangeYield ? (
                    <RoomChangeYieldPrompt message={message} onAnswer={answer => onRoomChangeYieldAnswer?.(message, answer)} />
                  ) : (
                    <ReactMarkdown
                      components={{ code: CodeBlock }}
                      remarkPlugins={[remarkGfm]}
                      rehypePlugins={[rehypeHighlight]}
                    >
                      {message.Content.toString() ?? ''}
                    </ReactMarkdown>
                  )}
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
};

const RoomChangeYieldPrompt: React.FC<{ message: WebSocketReplyChatRoomMessage; onAnswer?: (answer: string) => void }> = ({ message, onAnswer }) => {
  const [answer, setAnswer] = React.useState<string | null>(null);

  const handleClick = (ans: string) => {
    setAnswer(ans);
    if (onAnswer) onAnswer(ans);
  };

  if (answer) {
    return (
      <div>
        <div>{message.Content.toString() ?? ''}</div>
        <div className="room-change-yield-answer">You answered: <b>{answer}</b></div>
      </div>
    );
  }

  return (
    <div>
      <div>{message.Content.toString() ?? ''}</div>
      <div style={{ marginTop: 8 }}>
        <button onClick={() => handleClick('Yes')}>Yes</button>
        <button onClick={() => handleClick('No')} style={{ marginLeft: 8 }}>No</button>
      </div>
    </div>
  );
};

export default memo(MessageList);
