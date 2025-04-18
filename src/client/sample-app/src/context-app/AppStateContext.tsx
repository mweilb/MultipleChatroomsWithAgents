import { createContext, useContext, useState, ReactNode, useEffect } from 'react';
import { useWebSocketContext } from 'shared';
import { WebSocketNewRoomMessage,WebSocketReplyChatRoomMessage } from 'shared';
 

interface AppStateContextProps {
  activeChatRoomName: string;
  setActiveChatRoomName: (chatRoomName: string) => void;
  activeChatSubRoomName: string;
  setActiveChatSubRoomName: (subRoomName: string) => void;
  availableRoomNames: string[];
  setAvailableRoomNames: (roomNames: string[]) => void;
  activeChannel: number;
  setActiveChannel: (channel: number) => void;
  getMessagesForChannel: () => WebSocketReplyChatRoomMessage[];
  requestRoomChange: boolean;
  requestRoomPause: boolean;
  requestRestart: boolean;
  setDidRoomChange: (change: roomChangePrompt) => void;
  nextRoom: string;
}

const AppStateContext = createContext<AppStateContextProps | undefined>(undefined);

interface AppStateProviderProps {
  children: ReactNode;
}

type roomChangePrompt = 'noSignal' | 'noChange' | 'changeRoom' | 'waiting';

export const AppStateProvider = ({ children }: AppStateProviderProps) => {
  const [activeChatRoomName, setActiveChatRoomName] = useState<string>('');
  const [activeChatSubRoomName, setActiveChatSubRoomName] = useState<string>('');
  const [availableRoomNames, setAvailableRoomNames] = useState<string[]>([]);
  const [activeChannel, setActiveChannel] = useState<number>(0);
  const [requestRoomChangeFlag, setRequestRoomChangeFlag] = useState<boolean>(false);
  const [requestRoomPauseFlag, setRequestRoomPauseFlag] = useState<boolean>(false);
  const [requestRestartFlag, setRequestRestartFlag] = useState<boolean>(false);
  const [didRoomChange, setDidRoomChange] = useState<roomChangePrompt>('noSignal');
  const [nextRoom, setNextRoom] = useState<string>('');
 
  // Get the subroom change listener and getMessages function from the WebSocket context.
  const { requestNewRoomListener, getMessages, requestRoomChange} = useWebSocketContext();

  // When availableRoomNames are set (or updated) and no subroom is selected yet,
  // default to the first subroom (index 0).
  useEffect(() => {
    if (availableRoomNames.length > 0 && activeChatRoomName === '') {
      setActiveChatSubRoomName(availableRoomNames[0]);
      setActiveChannel(0);
    }
  }, [activeChatRoomName, availableRoomNames]);
  
  // Listen for subroom change messages.
  useEffect(() => {
    requestNewRoomListener((msg: WebSocketNewRoomMessage) => {
      if (msg.SubAction === "change-room") {
        setNextRoom(msg.To);
        setRequestRoomPauseFlag(true);
      }
      else {
        console.log("Room change request recieved")      
        setNextRoom(msg.To);
        setRequestRoomChangeFlag(true);
      }
    });
  }, [requestNewRoomListener]);

  useEffect(() => {
    
    if (didRoomChange === "changeRoom") {1
      console.log("Changing room to:", nextRoom);
      
      setActiveChatSubRoomName(nextRoom);

      if (requestRoomChangeFlag) {
        requestRoomChange("userid",activeChatRoomName, nextRoom);
      }

      const index = availableRoomNames.indexOf(nextRoom);
      if (index !== -1) {
        setActiveChannel(index);
      } else {
        console.warn("Sub room not found:", nextRoom);
      }
      
      // Clear pending request after processing the change
      setRequestRoomChangeFlag(false);
      setRequestRoomPauseFlag(false);
      setRequestRestartFlag(false);
      setDidRoomChange("noSignal");
    
    } else if (didRoomChange === "noChange") {
      console.log("Room change declined; staying in:", activeChatSubRoomName);
      // Clear pending request after processing the change
      setRequestRoomChangeFlag(false);
      setRequestRoomPauseFlag(false);
      setRequestRestartFlag(false);
      setDidRoomChange("noSignal");
    }

    

  }, [didRoomChange, nextRoom, availableRoomNames, activeChatSubRoomName]);  

  // Function to retrieve messages for the active channel.
  const getMessagesForChannel = (): WebSocketReplyChatRoomMessage[] => {
    // Get all messages for the active chat room.
    const messages = getMessages(activeChatRoomName);
    // Return only those messages where both the RoomName and SubRoomName match.
    return messages.filter(
      (msg) =>
        msg.RoomName === activeChatSubRoomName &&
        (msg.SubAction === 'reply' || msg.SubAction === 'ask')  
    );
  };

  return (
    <AppStateContext.Provider
      value={{
        activeChatRoomName,
        setActiveChatRoomName,
        activeChatSubRoomName,
        setActiveChatSubRoomName,
        availableRoomNames,
        setAvailableRoomNames,
        activeChannel,
        setActiveChannel,
        getMessagesForChannel,
        requestRoomChange: requestRoomChangeFlag,
        requestRoomPause: requestRoomPauseFlag,
        requestRestart: requestRestartFlag,
        setDidRoomChange,
        nextRoom,
      }}
    >
      {children}
    </AppStateContext.Provider>
  );
};

export const useAppStateContext = (): AppStateContextProps => {
  const context = useContext(AppStateContext);
  if (!context) {
    throw new Error('useAppStateContext must be used within an AppStateProvider');
  }
  return context;
};
