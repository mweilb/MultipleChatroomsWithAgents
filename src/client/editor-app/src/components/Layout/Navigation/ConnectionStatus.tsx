// ConnectionStatus.tsx
import React from 'react';
import { useWebSocketContext } from 'shared';
interface ConnectionStatusProps {
 
}

const ConnectionStatus: React.FC<ConnectionStatusProps> = () => {
  
const { connectionStatus } = useWebSocketContext();

  return <p className="connection-status">Status: {connectionStatus}</p>;
};

export default ConnectionStatus;
