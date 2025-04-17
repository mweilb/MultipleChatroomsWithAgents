import  { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import logo from "./assets/peckham-logo-vertical.png";
import "./BeforeWeStart.css";
import { useWebSocketContext } from 'shared';
import { useAppStateContext } from './context-app/AppStateContext';
 

const BeforeWeStart = () => {
  const navigate = useNavigate();
  const { rooms, toggleVoice , resetChat} = useWebSocketContext();
  const { setActiveChatRoomName, setAvailableRoomNames, setActiveChatSubRoomName,setActiveChannel } = useAppStateContext();
  const [selectedRoom, setSelectedRoom] = useState<string>('');

  // Filter rooms to only include those that have exactly 4 items in the nested Rooms array.
  const filteredRooms = rooms.filter(room => room.Rooms && room.Rooms.length <= 5);

  const getAvailableRoomNames = (targetRoom: String) => {
    return filteredRooms.find(room => room.Name === targetRoom)?.Rooms.map(subRoom => subRoom.Name) || [];
  }

  const goToLanding = () => {
    if (selectedRoom) {
      // Send reset event before any state changes or navigation.
      resetChat(selectedRoom);
      setActiveChatRoomName(selectedRoom);
      setAvailableRoomNames(getAvailableRoomNames(selectedRoom));
      setActiveChannel(0);
      setActiveChatSubRoomName(getAvailableRoomNames(selectedRoom)[0]);
      toggleVoice(true);
      navigate('/chatroom', { state: { room: selectedRoom } });
    } else {
      alert('Please select a chat room');
    }
  };

  return (
    <div className="container">
      <div className="title-container">
        <h1 className="main-title"> Multi Agent Chat Demo </h1>
        <h2 className="title"> AI Job Matching Platform </h2>
      </div>

      <div className="dropdown-container">
        <div className="dropdown-label">
          Select a Chatroom:
        </div>
        <select
          id="chatroom-select"
          value={selectedRoom}
          onChange={(e) => setSelectedRoom(e.target.value)}
          className="dropdown-select"
        >
          <option value="">Select</option>
          {filteredRooms.map((room) => (
            <option key={room.Name} value={room.Name}>
              {room.Name}
            </option>
          ))}
        </select>
      </div>

      <div className="button-container">
        <button
          className="landing-button"
          onClick={goToLanding}
          disabled={!selectedRoom} // Disable button until a chat room is selected
        >
          Start The Experience
        </button>
      </div>
    </div>
  );
};

export default BeforeWeStart;
