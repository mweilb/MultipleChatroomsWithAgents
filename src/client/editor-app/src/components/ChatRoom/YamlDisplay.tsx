import React from "react";
import remarkGfm from "remark-gfm";
import rehypeHighlight from "rehype-highlight";

import { CodeBlock } from "./CodeBlock";

import { useWebSocketContext,WebSocketRoom } from "shared";
 
import ReactMarkdown from "react-markdown";
import "./YamlDisplay.css";

interface GraphOfChartRoomProps {
  roomName: string;
}

const YamlDisplay: React.FC<GraphOfChartRoomProps> = ({ roomName }) => {
  const { rooms } = useWebSocketContext();

  // Find the room with the exact roomName
  const room: WebSocketRoom | undefined = rooms.find(
    (r) => r.Name === roomName
  );
  
  // Wrap the YAML content in markdown code fences for proper syntax highlighting.
  const yamlContent = room ? `\`\`\`yaml
${room.Yaml}
\`\`\`` : "No YAML found.";

  return (
    <div className="full-page-container">
      <div className="yaml-container">
 
        {room && room.Errors && room.Errors.length > 0 && (
          <div className="errors-container">
            <h3>Errors:</h3>
            <ul>
              {room.Errors.map((error, index) => (
                <li 
                  key={index} 
                  className={`error-row error-row-${index % 3}`}
                >
                  {error}
                </li>
              ))}
            </ul>
          </div>
        )}
        <ReactMarkdown
          components={{ code: CodeBlock }}
          remarkPlugins={[remarkGfm]}
          rehypePlugins={[rehypeHighlight]}
        >
          {yamlContent}
        </ReactMarkdown>
      </div>
    </div>
  );
};

export default YamlDisplay;
