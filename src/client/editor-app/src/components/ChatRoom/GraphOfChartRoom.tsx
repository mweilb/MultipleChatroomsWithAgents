import React from "react";
import MermaidComponent from "./MermaidComponent";
import "./GraphOfChartRoom.css";
import { useWebSocketContext,WebSocketRoom } from "shared";
 
 

interface GraphOfChartRoomProps {
  roomName: string;
}

const GraphOfChartRoom: React.FC<GraphOfChartRoomProps> = ({ roomName }) => {
  const { rooms } = useWebSocketContext();

  // Find the room with the exact roomName
  const room: WebSocketRoom | undefined = rooms.find(
    (r) => r.Name === roomName
  );

  // Use the room's Mermaid graph if available; otherwise, use a fallback graph
  const graphDefinition = room?.MerMaidGraph || `
    graph LR
      A[Room: ${roomName}] --> B[No Mermaid Graph available]
  `;

  return (
    <div className="full-page-container">
      <div className="graph-container">
        <MermaidComponent chart={graphDefinition} />
      </div>
    </div>
  );
};

export default GraphOfChartRoom;
