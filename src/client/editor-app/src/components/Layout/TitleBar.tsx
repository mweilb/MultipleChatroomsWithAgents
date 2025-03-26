import React from 'react';
import './TitleBar.css';

const TitleBar: React.FC = () => {
  return (
    <header className="title-bar">
      <div className="logo">
        {/* Replace with your logo image or text */}
        🤖🤖 💬 👥
      </div>
      <div className="title-text">
        <h1>Multi-Agent Prototypes</h1>
      </div>
      <div className="title-links">
        <a href="/about">About</a>
        <a href="/contact">Contact</a>
        {/* Add more links as needed */}
      </div>
    </header>
  );
};

export default TitleBar;
