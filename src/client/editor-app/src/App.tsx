import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';

import TitleBar from './components/Layout/TitleBar';
import Navigator from './components/Layout/Navigation/Navigator';
import { navItems } from './configs/NavigationItems';
import { WebSocketProvider } from 'shared';
import DynamicChartRoomRouter from './components/Layout/Navigation/DynamicChartRoomRouter';
import IntroPage from './components/IntroTools/IntroPage';
import DynamicLibrarianRouter from './components/Layout/Navigation/DynanicLibrarianRouter';
 
import './App.css';

const App: React.FC = () => {
  return (
    <WebSocketProvider url="ws://127.0.0.1:5000/ws?token=expected_token" appType="editor">
      <Router>
        <div className="app-wrapper">
          <TitleBar />
          <div className="app-container">
            <Navigator />
            <div className="main-content">
              <Routes>
                {navItems.map((item) => (
                  <Route key={item.path} path={item.path} element={item.element} />
                ))}
                  <Route path="/rooms/:roomName" element={<DynamicChartRoomRouter />} />
                  <Route path="/library/:roomName/:librarianName" element={<DynamicLibrarianRouter />} />
    
                  <Route path="/" element={<IntroPage />} />
              </Routes>
            </div>
          </div>
        </div>
      </Router>
    </WebSocketProvider>
  );
};

export default App;
