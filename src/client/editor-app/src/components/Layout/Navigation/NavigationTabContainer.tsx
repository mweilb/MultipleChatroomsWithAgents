import React, { useState } from 'react';
import DynamicRoomsList from './DynamicRoomsList';
import './Navigator.css';
import DynamicLibrarianList from './DynamicLibrarianList';

interface NavigationTabContainerProps {
  location: any; // You can replace 'any' with a proper type if available (e.g., Location from 'react-router-dom')
  isCollapsed: boolean;
}

const NavigationTabContainer: React.FC<NavigationTabContainerProps> = ({ location, isCollapsed }) => {
  const [activeTab, setActiveTab] = useState<'rooms' | 'libraries'>('rooms');

  return (
    <div className={`navigation-tab-container ${isCollapsed ? 'collapsed' : ''}`}>
      {/* Only show tab header when not collapsed */}
      {!isCollapsed && (
        <div className="navigation-tab-header">
          <button
            onClick={() => setActiveTab('rooms')}
            className={`navigation-tab-button ${activeTab === 'rooms' ? 'active' : ''}`}
          >
            Rooms
          </button>
          <button
            onClick={() => setActiveTab('libraries')}
            className={`navigation-tab-button ${activeTab === 'libraries' ? 'active' : ''}`}
          >
            Librarians
          </button>
        </div>
      )}
      <div className="navigation-tab-content">
        {activeTab === 'rooms' && (
          <DynamicRoomsList isCollapsed={isCollapsed} location={location} />
        )}
        {activeTab === 'libraries' && (
           <DynamicLibrarianList isCollapsed={isCollapsed} location={location} />)}
      </div>
    </div>
  );
};

export default NavigationTabContainer;
