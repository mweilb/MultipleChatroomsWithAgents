import React from 'react';
import "./Moderation.css";
import { useWebSocketContext,WebSocketModeration } from 'shared';

interface ModerationProps {
  roomName: string;
}

const Moderation: React.FC<ModerationProps> = ({ roomName }) => {
  // Retrieve the moderationHistory from the WebSocket context.
  // moderationHistory maps room names to arrays of moderation messages.
  const { moderationHistory } = useWebSocketContext();

  // Get the messages for the given room and for the "all" key.
  // If roomName is "all", we only use the messages under "all".
  const roomMessages: WebSocketModeration[] = roomName !== "all" 
    ? (moderationHistory[roomName] || [])
    : [];
  const allMessages: WebSocketModeration[] = moderationHistory["all"] || [];

  // If a specific room is provided, combine the room's messages with the "all" messages.
  // Otherwise, if roomName is "all", we simply show the "all" messages.
  const messages: WebSocketModeration[] = roomName !== "all"
    ? [...roomMessages, ...allMessages]
    : allMessages;

  return (
    <div className="moderation-container">
      <h2 className="moderation-header">
        Moderation Messages {roomName !== "all" ? `for ${roomName}` : "(All)"}
      </h2>
      <div className="moderation-table-wrapper">
        {messages.length === 0 ? (
          <p className="moderation-empty">No moderation messages received yet.</p>
        ) : (
          <table className="moderation-table">
            <thead>
              <tr>
                <th className="moderation-col-transaction">TransactionId</th>
                <th className="moderation-col-why">Room</th>
                <th className="moderation-col-content">Content</th>
              </tr>
            </thead>
            <tbody>
              {messages.map((msg, index) => (
                <tr key={index}>
                  <td className="moderation-col-transaction">{msg.TransactionId}</td>
                  <td className="moderation-col-why">{msg.Why}</td>
                  <td className="moderation-col-content">{msg.Content}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

export default Moderation;
