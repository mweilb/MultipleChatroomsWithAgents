import React, { useState, useEffect, useCallback } from 'react';
import './ChatRoom.css';
import { useWebSocketContext } from 'shared'; // Custom hook for WebSocket context

interface ChatInputProps {
  input: string;                             /** The current value of the input field */
  onInputChange: (value: string) => void;    /** Callback to update the input state when the user types */
  onSend: () => void;                       /** Callback to send the message */
  onKeyDown: React.KeyboardEventHandler<HTMLTextAreaElement>; /** Callback to handle keyboard events, like Enter key to send message */
}

const MAX_ROWS = 5; // Maximum number of rows the textarea can grow to

const ChatInput: React.FC<ChatInputProps> = ({
  input,
  onInputChange,
  onSend,
  onKeyDown,
}) => {
  // State to manage the number of rows for the textarea (grows dynamically with input)
  const [rows, setRows] = useState(1);

  // Get the WebSocket connection status from the context
  const { connectionStatus } = useWebSocketContext();

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

  return (
    <div className="chat-input">
      <textarea
        rows={rows} // Dynamically set the rows based on input
        value={input} // Set the value of the textarea to the input state
        onChange={(e) => onInputChange(e.target.value)} // Update the input state when the user types
        onKeyDown={onKeyDown} // Handle keyboard events (e.g., Enter to send)
        placeholder={
          connectionStatus === 'Connected'
            ? 'Type a message' // Show message when connected
            : 'Waiting for connection...' // Show message when not connected
        }
        disabled={connectionStatus !== 'Connected'} // Disable input if not connected
      />
      <button onClick={handleSend} disabled={connectionStatus !== 'Connected'}>
        Send
      </button>
    </div>
  );
};

export default ChatInput;
