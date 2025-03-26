import React, { useState } from 'react';
import './Librarian.css';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import { CodeBlock } from '../ChatRoom/CodeBlock';
import { WebSocektLibrainDocRef } from 'shared';

// Component to display an individual reference with toggle for the text.
export const ReferenceItem: React.FC<{ reference: WebSocektLibrainDocRef }> = ({ reference }) => {
  const [isTextVisible, setTextVisible] = useState(false);
  const [isAnswerVisible, setAnswerVisible] = useState(false);

  const toggleText = () => {
    setTextVisible((prev) => !prev);
  };

  const toggleAnswer = () => {
    setAnswerVisible((prev) => !prev);
  };

  return (
    <div className="reference">
      <p>
        <span className="reference-label">Score:</span> {reference.Score}
      </p>
      <div className="link-container">
        <div>
          <span className="reference-label">Link:</span>
          <a href={reference.DocumentUri} target="_blank" rel="noopener noreferrer">
            {reference.DocumentUri}
          </a>
        </div>
        <button onClick={toggleText}>
          {isTextVisible ? 'Hide Text' : 'Show Text'}
        </button>
      </div>
      {isTextVisible && (
        <div className="reference-text scrollable">
          <ReactMarkdown
            
            components={{ code: CodeBlock }}
            remarkPlugins={[remarkGfm]}
            rehypePlugins={[rehypeHighlight]}
          >
            {reference.Text}
          </ReactMarkdown>
        </div>
      )}
      <div className="question-container">
        <div>
          <span className="reference-label">Question:</span> {reference.Question}
        </div>
        <button onClick={toggleAnswer}>
          {isAnswerVisible ? 'Hide Answer' : 'Show Answer'}
        </button>
      </div>
      {isAnswerVisible && (
        <div className="reference-text scrollable">
          <p>
            <span className="reference-label">Answer:</span>
          </p>
          <ReactMarkdown
            
            components={{ code: CodeBlock }}
            remarkPlugins={[remarkGfm]}
            rehypePlugins={[rehypeHighlight]}
          >
            {reference.Answer}
          </ReactMarkdown>
        </div>
      )}
    </div>
  );
};
