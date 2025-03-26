import { useState } from "react";
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

  // Get moderation and error histories from the context.
  const { moderationHistory, errorHistory } = useWebSocketContext();

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
          Yaml
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
          Errors {errorCount > 0 && <span className="badge">{errorCount}</span>}
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
        {activeTab === 'text' && <YamlDisplay roomName={`${roomName}`} />}
        {activeTab === 'moderation' && <Moderation roomName={`${roomName}`} />}
        {activeTab === 'error' && <ErrorPage roomName={`${roomName}`} />}
      </div>
    </div>
  );
};

export default TabContainer;
