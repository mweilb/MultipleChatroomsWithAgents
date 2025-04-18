import React, { useState, useEffect, useCallback } from 'react';
import './ChatRoom.css';
import { useWebSocketContext } from 'shared'; // Custom hook for WebSocket context

interface ChatInputProps {
  input: string;                             /** The current value of the input field */
  onInputChange: (value: string) => void;    /** Callback to update the input state when the user types */
  onSend: () => void;                       /** Callback to send the message */
  onKeyDown: React.KeyboardEventHandler<HTMLTextAreaElement>; /** Callback to handle keyboard events, like Enter key to send message */
  actionKey?: string;                        /** Action key for request state tracking */
}

const MAX_ROWS = 8; // Maximum number of rows the textarea can grow to

const ChatInput: React.FC<ChatInputProps> = ({
  input,
  onInputChange,
  onSend,
  onKeyDown,
  actionKey,
}) => {
  // State to manage the number of rows for the textarea (grows dynamically with input)
  const [rows, setRows] = useState(1);

  // Get the WebSocket connection status from the context
  const { connectionStatus, getRequestState } = useWebSocketContext();

  // Dynamically adjust the textarea row count based on the number of lines in the input
  useEffect(() => {
    const lineCount = input.split('\n').length; // Count the number of lines in the input
    setRows(Math.min(lineCount, MAX_ROWS)); // Cap the rows to the MAX_ROWS limit
  }, [input]);

  // Handle the message send action, ensuring the WebSocket is connected
  const handleSend = useCallback(() => {
    if (connectionStatus !== 'Connected') return; // Prevent sending if not connected
    onSend(); // Trigger the send callback
    setRows(1); // Reset rows back to 1 after sending
  }, [onSend, connectionStatus]);

  const isRequestComplete =
    !actionKey || getRequestState(actionKey) === 'complete';

  return (
    <div className="chat-input">
      <textarea
        rows={rows}
        value={input}
        onChange={(e) => onInputChange(e.target.value)}
        onKeyDown={(e) => {if (isRequestComplete ) {onKeyDown(e) }}}
        placeholder={
          connectionStatus === 'Connected'
            ? 'Type your message here… 😊'
            : 'Waiting for connection…'
        }
        className="chat-input-textarea"
      />
      <button
        onClick={handleSend}
        disabled={
          connectionStatus !== 'Connected' || !isRequestComplete  
        }
        className="chat-input-send-btn"
        aria-label="Send"
      >
        <svg width="22" height="22" viewBox="0 0 22 22" fill="none" style={{marginRight: '0.3em'}} xmlns="http://www.w3.org/2000/svg">
          <path d="M2 20L20 11L2 2V9.5L15 11L2 12.5V20Z" fill="currentColor"/>
        </svg>
        Send
      </button>
    </div>
  );
};

export default ChatInput;
