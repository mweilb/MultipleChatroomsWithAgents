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
}

const MessageList: React.FC<MessageListProps> = ({ messages, showRationales }) => {
  return (
    <div className="message-list-container">
      <div className="messages">
        {messages.length === 0 ? (
          <p>No messages yet.</p>
        ) : (
          messages.map((message) => {
            // Determine the type of message
            const isQuestion = message.SubAction === 'ask';
            const isRoomChange = message.SubAction === 'change-room';
            const isAppEditor = message.Mode === 'Editor';

            // Use showRationales prop to control the display of editor messages.
            if (isAppEditor && !showRationales) {
              return null;
            }

            if (isRoomChange) {
              const roomMessage = message as any;
              return (
                <div key={roomMessage.TransactionId} className="message room-change">
                  <div className="room-change-info">
                    <p>
                      Room changed from: <strong>{roomMessage.From}</strong> to{' '}
                      <strong>{roomMessage.To}</strong>
                    </p>
                    <ReactMarkdown
                      components={{ code: CodeBlock }}
                      remarkPlugins={[remarkGfm]}
                      rehypePlugins={[rehypeHighlight]}
                    >
                      {roomMessage.Content}
                    </ReactMarkdown>
                  </div>
                </div>
              );
            }

            return (
              <div
                key={message.TransactionId}
                className={`message ${isAppEditor ? 'rationale' : ''} ${isQuestion ? 'question' : ''}`}
              >
                {/* Render the agent's name if the message is not a question and is not from an Editor */}
                {!isQuestion && !isAppEditor && message.AgentName?.trim() && (
                  <div className="agent-info">
                    <div className="agent-name">{message.AgentName}</div>
                    <div className="agent-icon">{message.Emoji}</div>
                  </div>
                )}
                <div className="message-content">
                  <ReactMarkdown
                    components={{ code: CodeBlock }}
                    remarkPlugins={[remarkGfm]}
                    rehypePlugins={[rehypeHighlight]}
                  >
                    {message.Content.toString() ?? ''}
                  </ReactMarkdown>
                </div>
              </div>
            );
          })
        )}
      </div>
    </div>
  );
};

export default memo(MessageList);
