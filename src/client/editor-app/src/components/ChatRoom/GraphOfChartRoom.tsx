import React, { useState } from "react";
import MermaidComponent from "./MermaidComponent";
import "./GraphOfChartRoom.css";
import { useWebSocketContext, WebSocketRoom } from "shared";

interface GraphOfChartRoomProps {
  roomName: string;
}

const ORIENTATIONS = [
  { label: "Top Down", value: "TD" },
  { label: "Left Right", value: "LR" },
];

const GraphOfChartRoom: React.FC<GraphOfChartRoomProps> = ({ roomName }) => {
  const { rooms } = useWebSocketContext();
  const [orientation, setOrientation] = useState<"TD" | "LR">("LR");

  // Find the room with the exact roomName
  const room: WebSocketRoom | undefined = rooms.find(
    (r) => r.Name === roomName
  );

  // Use the room's Mermaid graph if available; otherwise, use a fallback graph
  let graphDefinition = room?.MerMaidGraph || `
    graph LR
      A[Room: ${roomName}] --> B[No Mermaid Graph available]
  `;

  // Replace the direction string in the Mermaid graph
  graphDefinition = graphDefinition.replace(
    /^graph\s+(TD|LR)/m,
    `graph ${orientation}`
  );

  return (
    <div className="full-page-container">
      <div className="card-container">
        <div className="orientation-row">
          <label htmlFor="orientation-select">
            Orientation:
          </label>
          <select
            id="orientation-select"
            value={orientation}
            onChange={(e) => setOrientation(e.target.value as "TD" | "LR")}
          >
            {ORIENTATIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>
        <div className="graph-container">
          <MermaidComponent chart={graphDefinition} />
        </div>
      </div>
    </div>
  );
};

export default GraphOfChartRoom;
