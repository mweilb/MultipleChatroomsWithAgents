import React, { useState } from "react";
 
import YamlDisplay from "../ChatRoom/YamlDisplay";
import ConverseWithLibrarianControl from "./ConverseWithLibrarianControl";
import ListWithLibrarianControl from "./ListWithLibrarianControl";
import DocsWithLibrarianControl from "./DocsWithLibrarianControl";

import "./Librarian.css"

interface LibrarianTabContainerProps {
  roomName: string | undefined;
  librarianName: string | undefined;
}

const LibrarianTabContainer: React.FC<LibrarianTabContainerProps> = ({ roomName, librarianName }) => {
  // Define the active tab state; one of 'converse', 'lookup', 'documents', or 'yaml'
  const [activeTab, setActiveTab] = useState<'converse' | 'lookup' | 'documents' | 'yaml'>('converse');

  return (
    <div className="librarian-tab-container">
      <div className="tab-header">
        <button 
          onClick={() => setActiveTab('converse')}
          className={`tab-button ${activeTab === 'converse' ? 'active' : ''}`}
        >
          Converse
        </button>
        <button 
          onClick={() => setActiveTab('lookup')}
          className={`tab-button ${activeTab === 'lookup' ? 'active' : ''}`}
        >
          Lookup
        </button>
        <button 
          onClick={() => setActiveTab('documents')}
          className={`tab-button ${activeTab === 'documents' ? 'active' : ''}`}
        >
          Documents
        </button>
        <button 
          onClick={() => setActiveTab('yaml')}
          className={`tab-button ${activeTab === 'yaml' ? 'active' : ''}`}
        >
          Yaml
        </button>
      </div>
      <div className="tab-content">
        {activeTab === 'converse' && (
           <ConverseWithLibrarianControl roomName={`${roomName}`} librarianName={`${librarianName}`}  />
        )}
        {activeTab === 'lookup' && (
           <ListWithLibrarianControl roomName={`${roomName}`} librarianName={`${librarianName}`}  />    
        )}
        {activeTab === 'documents' && (
            <DocsWithLibrarianControl roomName={`${roomName}`} librarianName={`${librarianName}`}  />    
        )}
        {activeTab === 'yaml' && (
          <YamlDisplay roomName={`${roomName}`} />
        )}
      </div>
    </div>
  );
};

export default LibrarianTabContainer;
