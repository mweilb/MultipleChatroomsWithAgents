import React from 'react';
import "./ErrorPage.css";
import { useWebSocketContext,WebSocketBaseMessage } from 'shared';
 

interface ErrorPageProps {
  roomName: string;
}

const ErrorPage: React.FC<ErrorPageProps> = ({ roomName }) => {
  // Retrieve the errorHistory from the WebSocket context.
  // errorHistory maps room names to arrays of error messages (WebSocketBaseMessage).
  const { errorHistory } = useWebSocketContext();

  // Get the error messages for the given room and for the "all" key.
  // If roomName is "all", we only use the messages under "all".
  const roomErrors: WebSocketBaseMessage[] = roomName !== "all"
    ? (errorHistory[roomName] || [])
    : [];
  const allErrors: WebSocketBaseMessage[] = errorHistory["all"] || [];

  // If a specific room is provided, combine the room's errors with the "all" errors.
  // Otherwise, if roomName is "all", we simply show the "all" errors.
  const messages: WebSocketBaseMessage[] = roomName !== "all"
    ? [...roomErrors, ...allErrors]
    : allErrors;

  return (
    <div className="error-container">
      <h2 className="error-header">
        Error Messages {roomName !== "all" ? `for ${roomName}` : "(All)"}
      </h2>
      <div className="error-table-wrapper">
        {messages.length === 0 ? (
          <p className="error-empty">No error messages received yet.</p>
        ) : (
          <table className="error-table">
            <thead>
              <tr>
                <th className="error-col-transaction">TransactionId</th>
                <th className="error-col-content">Content</th>
              </tr>
            </thead>
            <tbody>
              {messages.map((msg, index) => (
                <tr key={index}>
                  <td className="error-col-transaction">{msg.TransactionId}</td>
                  <td className="error-col-content">{msg.Content}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default ErrorPage;
