import React, { useState } from 'react';
import { Link } from 'react-router-dom';
 
import { useWebSocketContext, WebSocketRoom, WebSocketRoomProfile } from 'shared';

interface DynamicRoomsProps {
  isCollapsed: boolean;
  location: any;
}

const DynamicRoomsList: React.FC<DynamicRoomsProps> = ({ isCollapsed, location }) => {
  const { rooms,resetChat } = useWebSocketContext();
  // Object to track if a room has been clicked (true) keyed by room name
  const [clickedRooms, setClickedRooms] = useState<{ [key: string]: boolean }>({});

  // Room set function - call this only once per room when it is clicked for the first time
  const handleRoomSet = (room: WebSocketRoom | WebSocketRoomProfile) => {
    resetChat(room.Name);
 
  };

  // Recursive function to render rooms and nested rooms
  const renderRooms = (roomList: WebSocketRoom[] | WebSocketRoomProfile[], isParent = true) => {
    return roomList.map((room) => {
      // Handler that will be executed on click
      const handleClick = () => {
        if (!clickedRooms[room.Name]) {
          handleRoomSet(room);
          setClickedRooms(prev => ({ ...prev, [room.Name]: true }));
        }
      };

      return (
        <li
          key={room.Name}
          className={location.pathname === `/rooms/${encodeURIComponent(room.Name)}` ? 'active' : ''}
        >
          {isParent && (
            <Link 
              to={`/rooms/${encodeURIComponent(room.Name)}`}
              onClick={handleClick}
            >
              {room.Emoji && <span className="nav-emoji">{room.Emoji}</span>}
              {!isCollapsed && <span>{room.Name}</span>}
            </Link>
          )}
          {!isParent && !isCollapsed && (
            <div className="nested-rooms" onClick={handleClick}>
              {room.Emoji && <span className="nav-sub-emoji">{room.Emoji}</span>}
              {!isCollapsed && <span>{room.Name}</span>}
            </div>
          )}
          {/* Render nested rooms if they exist */}
          {(room as WebSocketRoom).Rooms && (room as WebSocketRoom).Rooms.length > 1 && (
            <ul className="nested-rooms">
              {renderRooms((room as WebSocketRoom).Rooms, false)}
            </ul>
          )}
        </li>
      );
    });
  };

  return (
    <div className="dynamic-rooms-container">
      <nav>
        <ul>
          {rooms.length > 1 ? (
            renderRooms(rooms)
          ) : (
            <li>
              <span className="no-rooms">No rooms available</span>
            </li>
          )}
        </ul>
      </nav>
    </div>
  );
};

export default DynamicRoomsList;
