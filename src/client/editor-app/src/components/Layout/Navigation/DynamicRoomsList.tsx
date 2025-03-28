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
  // Track if a room's subrooms are expanded (true) keyed by room name
  const [expandedRooms, setExpandedRooms] = useState<{ [key: string]: boolean }>({});

  // Pre-initialize each room with subrooms as collapsed (false)
  useEffect(() => {
    const initialExpandedState: { [key: string]: boolean } = {};
    rooms.forEach((room) => {
      initialExpandedState[room.Name] = false;
    });
    setExpandedRooms(initialExpandedState);
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
      const roomData = room as WebSocketRoom;
      const subrooms = roomData.Rooms || [];
      // If there is only one subroom and its name is the same as the parent, treat it as no subrooms.
      const hasSubrooms = subrooms.length > 0 && !(subrooms.length === 1 && subrooms[0].Name === room.Name);

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
              {!isCollapsed && <span>{room.Name}</span>}
            </Link>
          ) : (
            <div className="nested-rooms" onClick={handleClick}>
              {room.Emoji && <span className="nav-sub-emoji">{room.Emoji}</span>}
              {!isCollapsed && <span>{room.Name}</span>}
            </div>
          )}

          {/* Render toggle below the room title if there are subrooms */}
          {hasSubrooms && (
            <div
              className="toggle-container"
              onClick={(e) => {
                e.stopPropagation();
                e.preventDefault();
                setExpandedRooms(prev => ({
                  ...prev,
                  [room.Name]: !prev[room.Name]
                }));
              }}
              style={{ cursor: 'pointer', marginLeft: isParent ? '36px' : '35' }}
            >
              {!isCollapsed && (expandedRooms[room.Name] ? '▼ rooms' : '► rooms')}
            </div>
          )}

          {/* Render nested rooms only if they exist and are expanded */}
          {!isCollapsed && hasSubrooms && expandedRooms[room.Name] && (
            <ul className="nested-rooms">
              {renderRooms(subrooms, false)}
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
