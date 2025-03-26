import React from 'react';
import { useParams } from 'react-router-dom';
import LibrarianTabContainer from '../../Librarians/LibrarianTabContainer';
 

/**
 * DynamicLibrarianRouter is responsible for rendering the librarian view dynamically.
 * It extracts the room name and librarian name from the URL parameters using useParams.
 * You can fetch additional details about the librarian from your context or API as needed.
 */
const DynamicLibrarianRouter: React.FC = () => {
  // Extract roomName and librarianName from the URL.
  const { roomName, librarianName } = useParams<{ roomName: string; librarianName: string }>();

  //<TabContainer roomName={roomName} librarianName={librarianName} />
  // Alternatively, if you have a dedicated component for librarians, for example:
  // <LibrarianDetails roomName={roomName} librarianName={librarianName} />

  return (
    <LibrarianTabContainer roomName={roomName} librarianName={librarianName} />
   
  );
};

export default DynamicLibrarianRouter;
