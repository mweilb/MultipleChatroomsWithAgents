import { useState, useEffect } from "react";
import { useErrorStoreContext } from '../../context/ErrorStoreContext';
import ChatRoom from "../ChatRoom/ChatRoom";
import GraphOfChartRoom from "../ChatRoom/GraphOfChartRoom";
import YamlDisplay from "../ChatRoom/YamlDisplay";
import Moderation from "../ChatRoom/Moderation";
import ErrorPage from "../ChatRoom/ErrorPage";
import { useWebSocketContext } from 'shared';

import "./TabContainer.css";

interface TabContainerProps {
  roomName: string | undefined;
}

const TabContainer: React.FC<TabContainerProps> = ({ roomName }) => {
  // Expanded union type to include 'error'
  const [activeTab, setActiveTab] = useState<'chat' | 'graph' | 'text' | 'moderation' | 'error'>('chat');
  const [localYamlErrorCount, setLocalYamlErrorCount] = useState(0);

  // Get moderation and error histories from the context.
  const { moderationHistory, errorHistory, rooms } = useWebSocketContext();
  const { yamlErrorCount, setYamlErrorCount } = useErrorStoreContext();

  // Compute counts based on roomName.
  // If roomName is not "all", combine messages for that room with those under "all".
  const moderationCount =
    roomName && roomName !== "all"
      ? ((moderationHistory[roomName] || []).length + (moderationHistory["all"] || []).length)
      : (moderationHistory["all"] || []).length;

  const errorCount =
    roomName && roomName !== "all"
      ? ((errorHistory[roomName] || []).length + (errorHistory["all"] || []).length)
      : (errorHistory["all"] || []).length;

  const yamlCount = yamlErrorCount[roomName ?? ""] || 0;

  // Always update context YAML error count for the current room, even if YAML tab is not active
  useEffect(() => {
    if (!roomName) return;
    const room = rooms.find(r => r.Name === roomName);
    const count = room && Array.isArray(room.Errors) ? room.Errors.length : 0;
    setYamlErrorCount(roomName, count);
  }, [roomName, rooms, setYamlErrorCount]);

  return (
    <div className="tab-container">
      <div className="tab-header">
        <button 
          onClick={() => setActiveTab('chat')} 
          className={`tab-button ${activeTab === 'chat' ? 'active' : ''}`}
        >
          Chat Room
        </button>
        <button 
          onClick={() => setActiveTab('graph')}
          className={`tab-button ${activeTab === 'graph' ? 'active' : ''}`}
        >
          Graph
        </button>
        <button 
          onClick={() => setActiveTab('text')}
          className={`tab-button ${activeTab === 'text' ? 'active' : ''}`}
        >
          Yaml <span className={`badge${yamlCount === 0 ? " badge-green" : ""}`}>{yamlCount}</span>
        </button>
        <button 
          onClick={() => setActiveTab('moderation')}
          className={`tab-button ${activeTab === 'moderation' ? 'active' : ''}`}
        >
          Moderation {moderationCount > 0 && <span className="badge">{moderationCount}</span>}
        </button>
        <button 
          onClick={() => setActiveTab('error')}
          className={`tab-button ${activeTab === 'error' ? 'active' : ''}`}
        >
          Errors {errorCount > 0 && (
            <span className="badge">
              {errorCount}
            </span>
          )}
        </button>
      </div>
      <div className="tab-content-main">
        {activeTab === 'chat' && (
          <ChatRoom
            chatType={`${roomName}`}
            title={`Room: ${roomName}`}
            userId="John Doe"
          />
        )}
        {activeTab === 'graph' && <GraphOfChartRoom roomName={`${roomName}`} />}
        {activeTab === 'text' && (
          <YamlDisplay
            roomName={`${roomName}`}
            onErrorCountChange={setLocalYamlErrorCount}
          />
        )}
        {activeTab === 'moderation' && <Moderation roomName={`${roomName}`} />}
        {activeTab === 'error' && <ErrorPage roomName={`${roomName}`} />}
      </div>
    </div>
  );
};

export default TabContainer;
