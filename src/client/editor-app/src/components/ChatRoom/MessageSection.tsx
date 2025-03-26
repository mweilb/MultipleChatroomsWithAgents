import React, { useEffect, useCallback, useMemo } from 'react';
 
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import './ChatRoom.css';
import { CodeBlock } from './CodeBlock';

export interface MessageSectionProps {
  title: string;                     /** The section's main title (header). */
  sectionCategory?: string;          /** Optional category for section-specific toggling (group sections for bulk toggle). */
  styleKey: string;                  /** Custom style key for styling the section uniquely. */
  localStorageKey: string;           /** Key to persist the section's expanded/collapsed state in `localStorage`. */
  subtitle?: string;                 /** Optional subtitle providing additional context beneath the title. */
  children?: React.ReactNode;        /** Optional fallback content when no markdown is provided. */
  useMarkdown?: boolean;             /** Whether the content is in markdown format. */
  markdownContent?: string;          /** Content to be rendered as markdown if `useMarkdown` is true. */
}


/**
 * Custom hook to manage the expanded/collapsed state of the section with `localStorage` persistence.
 * This hook ensures that the section's state is maintained even after page reloads.
 *
 * @param key The key used to store the expanded state in `localStorage`.
 * @param defaultValue The default value to use if no state is found in `localStorage`.
 * @returns The current state and a function to update the state.
 */
function useLocalStorageState(key: string, defaultValue: boolean) {
  const [state, setState] = React.useState<boolean>(() => {
    try {
      const stored = localStorage.getItem(key);
      return stored !== null ? stored === 'true' : defaultValue;
    } catch {
      return defaultValue;
    }
  });

  // Update `localStorage` whenever the state changes
  useEffect(() => {
    try {
      localStorage.setItem(key, state.toString());
    } catch (error) {
      console.error(`Error saving to localStorage key "${key}":`, error);
    }
  }, [key, state]);

  return [state, setState] as const;
}

const MessageSection: React.FC<MessageSectionProps> = ({
  title,
  subtitle,
  sectionCategory,
  styleKey,
  localStorageKey,
  children,
  useMarkdown = false,
  markdownContent,
}) => {
  // Manage the expanded/collapsed state of the section, with persistence in localStorage
  const [isSectionExpanded, setSectionExpanded] = useLocalStorageState(localStorageKey, true);

  /**
   * Handles the toggle event to expand or collapse the section.
   * Listens for events and updates the expanded state based on the event details.
   *
   * @param event The event containing information about whether the section should be expanded.
   */
  const handleSectionExpandToggle = useCallback((event: Event) => {
    const customEvent = event as CustomEvent<{ category: string; isOpen: boolean }>;
    const { category, isOpen } = customEvent.detail;

    // Update section state only if the event matches the category (or if it's a global event)
    if (category === 'all' || (sectionCategory && category === sectionCategory)) {
      setSectionExpanded(isOpen);
    }
  }, [sectionCategory, setSectionExpanded]);

  // Set up event listener when the component is mounted and clean up on unmount
  useEffect(() => {
    window.addEventListener('message-section-toggle', handleSectionExpandToggle as EventListener);
    return () => {
      window.removeEventListener('message-section-toggle', handleSectionExpandToggle as EventListener);
    };
  }, [handleSectionExpandToggle]);

  // Memoize content rendering to optimize performance (only re-render when necessary)
  const renderedContent = useMemo(() => {
    // If markdown is enabled, render the content as markdown
    if (useMarkdown && markdownContent) {
      return (
    
          <ReactMarkdown
             
            components={{ code: CodeBlock }}
            remarkPlugins={[remarkGfm]}
            rehypePlugins={[rehypeHighlight]}
          >
            {markdownContent}
          </ReactMarkdown>
       
      );
    }

    // If markdownContent exists but markdown rendering is not enabled, render it as plain text
    if (markdownContent) {
      return <span>{markdownContent}</span>;
    }

    // Fallback: render any children passed to the component
    return children;
  }, [useMarkdown, markdownContent, children]);

  return (
    <div className="collapsible-section">
      <div className={`collapsible-header ${styleKey}`}>
        <button
          className="message-toggle-button"
          onClick={() => setSectionExpanded(prev => !prev)} // Toggle the expanded state
          aria-label={isSectionExpanded ? 'Collapse section' : 'Expand section'}
          aria-expanded={isSectionExpanded}
        >
          {isSectionExpanded ? "▼" : "►"}
        </button>
        <span className="collapsible-label-wrapper">
          <span className="collapsible-label">{title}</span>
          {subtitle && <span className="collapsible-sublabel">{subtitle}</span>}
        </span>
      </div>
      {isSectionExpanded && <div className="collapsible-content">{renderedContent}</div>}
    </div>
  );
};

export default MessageSection;
