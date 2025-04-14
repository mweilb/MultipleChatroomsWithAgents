import React from 'react';
import { useLocation } from 'react-router-dom';
 
import { navItems } from '../../../configs/NavigationItems';
 
import AdminNav from './AdminNav';
import ConnectionStatus from './ConnectionStatus';
import NavigationTabContainer from './NavigationTabContainer';
import './Navigator.css';

interface NavigatorProps {
  isCollapsed: boolean;
  setIsCollapsed: React.Dispatch<React.SetStateAction<boolean>>;
}

const Navigator: React.FC<NavigatorProps> = ({ isCollapsed, setIsCollapsed }) => {
  const location = useLocation();


  // Toggle the navigation collapse state
  const toggleNav = () => setIsCollapsed(prev => !prev);

  return (
    <div className={`navigator ${isCollapsed ? 'collapsed' : ''}`}>
      <button onClick={toggleNav} className="toggle-button">
        {isCollapsed ? '«' : '»'}
      </button>

      <NavigationTabContainer isCollapsed={isCollapsed} location={location}/>
       
      
 
      
      <AdminNav isCollapsed={isCollapsed} navItems={navItems} location={location} />
      <ConnectionStatus />
    </div>
  );
};

export default Navigator;
