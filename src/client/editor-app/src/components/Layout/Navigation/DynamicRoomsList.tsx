import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';

import { useWebSocketContext, WebSocketRoom, WebSocketRoomProfile } from 'shared';
 

interface DynamicRoomsProps {
  isCollapsed: boolean;
  location: any;
}

const DynamicRoomsList: React.FC<DynamicRoomsProps> = ({ isCollapsed, location }) => {
  const { rooms, resetChat } = useWebSocketContext();
  // Track if a room has been clicked (true) keyed by room name
  const [clickedRooms, setClickedRooms] = useState<{ [key: string]: boolean }>({});


  // Pre-initialize each room with subrooms as collapsed (false)
  useEffect(() => {
    const initialExpandedState: { [key: string]: boolean } = {};
    rooms.forEach((room) => {
      initialExpandedState[room.Name] = false;
    });

  }, [rooms]);

  // Call this only once per room when it is clicked for the first time
  const handleRoomSet = (room: WebSocketRoom | WebSocketRoomProfile) => {
    resetChat(room.Name);
  };

  // Recursive function to render rooms and nested rooms
  const renderRooms = (roomList: WebSocketRoom[] | WebSocketRoomProfile[], isParent = true) => {
    return roomList.map((room) => {
      // Handler for room item click
      const handleClick = () => {
        if (!clickedRooms[room.Name]) {
          handleRoomSet(room);
          setClickedRooms(prev => ({ ...prev, [room.Name]: true }));
        }
      };

      // Determine subrooms for this room
      return (
        <li
          key={room.Name}
          className={location.pathname === `/rooms/${encodeURIComponent(room.Name)}` ? 'active' : ''}
        >
          {isParent ? (
            <Link
              to={`/rooms/${encodeURIComponent(room.Name)}`}
              onClick={handleClick}
            >
              {room.Emoji && <span className="nav-emoji">{room.Emoji}</span>}
              {!isCollapsed && <span className="nav-room">{room.Name}</span>}
            </Link>
          ) : (
            <div className="nested-rooms" onClick={handleClick}>
              {room.Emoji && <span className="nav-sub-emoji">{room.Emoji}</span>}
              {!isCollapsed && <span className="nav-sub-room">{room.Name}</span>}
            </div>
          )}

           
        </li>
      );
    });
  };

  return (
    <div className="dynamic-rooms-container">
      <nav>
        <ul>
          {rooms.length > 0 ? (
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
