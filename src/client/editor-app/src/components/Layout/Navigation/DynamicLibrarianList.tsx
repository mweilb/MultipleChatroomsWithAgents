import React from 'react';
import { Link } from 'react-router-dom';
import { useWebSocketContext } from 'shared';
import './Navigator.css';

interface DynamicLibrarianListProps {
  isCollapsed: boolean;
  location: any;
}

const DynamicLibrarianList: React.FC<DynamicLibrarianListProps> = ({ isCollapsed, location }) => {
  // Retrieve library data from the WebSocket context.
  const { library } = useWebSocketContext();

  // If there is no library or no rooms exist, show a fallback message.
  if (!library || !library.Rooms || library.Rooms.length === 0) {
    return (
      <div className="dynamic-library-container">
        <nav>
          <ul>
            <li>
              <span className="no-library">No librarians available</span>
            </li>
          </ul>
        </nav>
      </div>
    );
  }

  // Flatten the librarians across all rooms into one list.
  const librarianEntries = library.Rooms.reduce<React.ReactElement[]>((acc, room) => {
    // Create list items for active librarians as clickable links.
    const activeEntries = room.ActiveLibrarians.map((librarian) => (
      <li key={`${room.Name}-${librarian.Name}`}>
        <Link
          to={`/library/${encodeURIComponent(room.Name)}/${encodeURIComponent(librarian.Name)}`}
        >
          {librarian.Emoji ? (
            <span className="librarian-emoji">{librarian.Emoji}</span>
          ) : (
            <span className="nav-icon">🏠</span>
          )}
          {!isCollapsed && <span>{`${librarian.Name}@${room.Name + room.Emoji}`}</span>}
        </Link>
      </li>
    ));

    // Create list items for not active librarians (disabled, with tooltip).
    const notActiveEntries = room.NotActiveLibrarians.map((librarian) => (
      <li key={`${room.Name}-${librarian.Name}`} className="disabled" title="This librarian is not active">
        {librarian.Emoji ? (
          <span className="librarian-emoji">{librarian.Emoji}</span>
        ) : (
          <span className="nav-icon">🏠</span>
        )}
        {!isCollapsed && <span>{`${librarian.Name}@${room.Name + room.Emoji}`}</span>}
      </li>
    ));

    return acc.concat(activeEntries, notActiveEntries);
  }, []);

  return (
    <div className="dynamic-library-container">
      <nav>
        <ul>
          {librarianEntries}
        </ul>
      </nav>
    </div>
  );
};

export default DynamicLibrarianList;
