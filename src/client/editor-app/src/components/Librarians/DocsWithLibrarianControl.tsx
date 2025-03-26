import React, { useState, useEffect } from 'react';
 
import './Librarian.css';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import { CodeBlock } from '../ChatRoom/CodeBlock';
import { useWebSocketContext, WebSocektLibrainDocRef } from 'shared';
import { ReferenceItem } from './ReferenceItem';

interface DocsWithLibrarianControlProps {
  roomName: string;
  librarianName: string;
}

const STORAGE_KEY = 'showReferencesMap';

const DocsWithLibrarianControl: React.FC<DocsWithLibrarianControlProps> = ({ roomName, librarianName }) => {
  // Initialize state from localStorage
  const [showReferencesMap, setShowReferencesMap] = useState<{ [transactionId: string]: boolean }>(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    return stored ? JSON.parse(stored) : {};
  });
  const [top, setTop] = useState<number>(10);  // Default top value
  const [skip, setSkip] = useState<number>(0);   // Default skip value

  // Save state changes to localStorage whenever it updates.
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(showReferencesMap));
  }, [showReferencesMap]);

  // Get functions from context.
  const { askForDocs, getLibrarianDocs } = useWebSocketContext();

  // Retrieve conversation messages from the store.
  const msg = getLibrarianDocs(roomName, librarianName);

  // Handler for sending a message.
  const handleSendMessage = () => {
    // Validate that top is above 1 and skip is non-negative.
    const validTop = top > 1 ? top : 2;
    const validSkip = skip >= 0 ? skip : 0;
    askForDocs(roomName, librarianName, validTop, validSkip);
  };

  return (
    <div className="converse-control-container">
      {/* Header */}
      <div className="converse-header">
        <div>{librarianName}@{roomName}</div>
      </div>

      {/* Controls for Top and Skip arranged in one row */}
      <div className="controls-row">
        <div className="input-group">
          <label htmlFor="topInput">Top:</label>
          <input
            id="topInput"
            type="number"
            min="2"
            value={top}
            onChange={(e) => setTop(Number(e.target.value))}
          />
        </div>
        <div className="input-group">
          <label htmlFor="skipInput">Skip:</label>
          <input
            id="skipInput"
            type="number"
            min="0"
            value={skip}
            onChange={(e) => setSkip(Number(e.target.value))}
          />
        </div>
        <button onClick={handleSendMessage}>Send Request</button>
      </div>

      {/* Conversation History Area */}
      <div className="conversation-history">
        {(!msg || Object.keys(msg).length === 0) ? (
          <p className="placeholder-text">Make a request for some documents!</p>
        ) : (
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
        )}
      </div>
    </div>
  );
};

export default DocsWithLibrarianControl;
