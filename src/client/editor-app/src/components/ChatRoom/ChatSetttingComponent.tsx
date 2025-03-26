import React, { useState } from 'react';
import { useWebSocketContext } from 'shared';
import './ChatSettingComponent.css';

interface ChatSettingComponentProps {
  currentRoomName: string;
}

const ChatSettingComponent: React.FC<ChatSettingComponentProps> = ({ currentRoomName }) => {
  const { toggleVoice, requestRoomChange, rooms, resetChat } = useWebSocketContext();
  const [voiceEnabled, setVoiceEnabled] = useState(false);
  const [selectedSubRoom, setSelectedSubRoom] = useState('');


  const handleVoiceToggle = () => {
    const newVoiceState = !voiceEnabled;
    setVoiceEnabled(newVoiceState);
    toggleVoice(newVoiceState);
  };

  // Find the current room by name and then extract its profiles.
  const currentRoom = rooms.find((room) => room.Name === currentRoomName);
  const subRooms = currentRoom?.Rooms || [];

  const handleRoomChange = () => {
    if (!selectedSubRoom) return;
    requestRoomChange(currentRoomName, selectedSubRoom);
  };

  // Call resetChat with the current room name.
  const handleResetChat = () => {
    resetChat(currentRoomName);
  };

  return (
    <div className="chat-setting compact">
      <div className="setting-item">
        <label className="setting-label">
          <input
            type="checkbox"
            checked={voiceEnabled}
            onChange={handleVoiceToggle}
          />
          Voice
        </label>
      </div>
      
      <div className="setting-item">
        <label htmlFor="subrooms" className="setting-label">
          Room
        </label>
        <select
          id="subrooms"
          value={selectedSubRoom}
          onChange={(e) => setSelectedSubRoom(e.target.value)}
          className="setting-select"
        >
          <option value="">-- Select a Room --</option>
          {subRooms.map((roomProfile, index) => (
            <option key={index} value={roomProfile.Name}>
              {roomProfile.Name} {roomProfile.Emoji ? roomProfile.Emoji : ''}
            </option>
          ))}
        </select>
      </div>
      
      <div className="setting-item">
        <button onClick={handleRoomChange} className="setting-button">
          Change Room
        </button>
      </div>
      
      <div className="setting-item">
        <button onClick={handleResetChat} className="setting-button">
          Reset Chat
        </button>
      </div>
    </div>
  );
};

export default ChatSettingComponent;
