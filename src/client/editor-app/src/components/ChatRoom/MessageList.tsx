import React, { memo, useEffect, useState } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import 'highlight.js/styles/github.css';
import './ChatRoom.css';
import MessageSections from './MessageSections';
import { WebSocketNewRoomMessage, WebSocketReplyChatRoomMessage } from 'shared';
import { CodeBlock } from './CodeBlock';
 

interface MessageListProps {
  messages: WebSocketReplyChatRoomMessage[]; // Array of chat messages to be displayed
  chatType: string; // Type of chat for styling or categorization (currently unused)
}

const MessageList: React.FC<MessageListProps> = ({ messages }) => {
  // State to track which sections and fields are collapsed
  const [collapsedSectionTitles, setCollapsedSectionTitles] = useState<Set<string>>(new Set());
  const [collapsedFieldLabels, setCollapsedFieldLabels] = useState<Set<string>>(new Set());

  useEffect(() => {
    /**
     * Event handler for toggling the visibility of message sections.
     * It adds or removes a section title from the collapsedSectionTitles state.
     */
    const onSectionToggle = (event: CustomEvent) => {
      const { sectionTitle, visible } = event.detail;
      setCollapsedSectionTitles((prevTitles) => {
        const updatedTitles = new Set(prevTitles);
        // If not visible, add the section to the collapsed state; otherwise, remove it.
        visible ? updatedTitles.delete(sectionTitle) : updatedTitles.add(sectionTitle);
        return updatedTitles;
      });
    };

    /**
     * Event handler for toggling the visibility of message fields.
     * It adds or removes a field label from the collapsedFieldLabels state.
     */
    const onFieldToggle = (event: CustomEvent) => {
      const { fieldLabel, visible } = event.detail;
      setCollapsedFieldLabels((prevLabels) => {
        const updatedLabels = new Set(prevLabels);
        // If not visible, add the field to the collapsed state; otherwise, remove it.
        visible ? updatedLabels.delete(fieldLabel) : updatedLabels.add(fieldLabel);
        return updatedLabels;
      });
    };

    // Attach event listeners for section and field toggling
    window.addEventListener('toggleSectionVisibility', onSectionToggle as EventListener);
    window.addEventListener('toggleFieldVisibility', onFieldToggle as EventListener);

    // Clean up event listeners on component unmount
    return () => {
      window.removeEventListener('toggleSectionVisibility', onSectionToggle as EventListener);
      window.removeEventListener('toggleFieldVisibility', onFieldToggle as EventListener);
    };
  }, []);

  return (
    <div className="message-list-container">
      <div className="messages">
        {messages.length === 0 ? (
          // Display a fallback message if no messages are available
          <p>No messages yet.</p>
        ) : (
          messages.map((message) => {
            // Determine if the message is a question based on its sub-action
            const isQuestion = message.SubAction === 'ask';
            const isRoomChange = message.SubAction === 'change-room';
 
            // If the message is a room-change, render its specific details.
            if (isRoomChange) {
              // Cast the message to WebSocketNewRoomMessage to access From and To
              const roomMessage = (message as any) as WebSocketNewRoomMessage;
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
                className={`message ${isQuestion ? 'question' : ''}`}
              >
                {/* Render the agent's name if the message is not a question and the agent name exists */}
                {!isQuestion && message.AgentName?.trim() && (
                  <div className="agent-info"> 
                    <div className="agent-name">{message.AgentName}</div>
                    <div className="agent-icon">{message.Emoji}</div>
                  </div>
                )}
                <div className="message-content">
                  {/* Render the message content using Markdown with GitHub-flavored markdown and syntax highlighting */}
                  <ReactMarkdown
                    components={{ code: CodeBlock }}
                    remarkPlugins={[remarkGfm]}
                    rehypePlugins={[rehypeHighlight]}
                  >
                   {message.Hints?.['agent']?.['content']?.toString() ?? ''}
                  </ReactMarkdown>
                  {/* Render additional message sections, passing down collapsed states */}
                  <MessageSections
                    msg={message}
                    collapsedSections={collapsedSectionTitles}
                    collapsedFields={collapsedFieldLabels}
                  />
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
