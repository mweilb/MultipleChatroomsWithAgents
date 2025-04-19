import { createContext, useContext, useState, ReactNode, useEffect, useReducer } from 'react';
import { useWebSocketContext } from 'shared';
import { WebSocketNewRoomMessage,WebSocketReplyChatRoomMessage } from 'shared';
 

interface AppStateContextProps {
  activeChatRoomName: string;
  setActiveChatRoomName: (chatRoomName: string) => void;
  activeChatSubRoomName: string;
  setActiveChatSubRoomName: (subRoomName: string) => void;
  availableRooms: { name: string; displayName: string }[];
  setAvailableRooms: (rooms: { name: string; displayName: string }[]) => void;
  activeChannel: number;
  getMessagesForChannel: () => WebSocketReplyChatRoomMessage[];
  requestRoomChange: RoomRequestFlag;
  requestRestart: boolean;
  setDidRoomChange: (change: RoomChangePrompt) => void;
}

type RoomRequestFlag = {
  flag: "pause" | "ask" | "";
  displayName: string;
  name: string;
};

type RoomRequestFlagStack = RoomRequestFlag[];

const AppStateContext = createContext<AppStateContextProps | undefined>(undefined);

interface AppStateProviderProps {
  children: ReactNode;
}

type RoomChangePrompt = 'noSignal' | 'noChange' | 'changeRoom' | 'waiting';

export const AppStateProvider = ({ children }: AppStateProviderProps) => {
  const [activeChatRoomName, setActiveChatRoomName] = useState<string>('');
  const [activeChatSubRoomName, setActiveChatSubRoomName] = useState<string>('');
  const [availableRooms, setAvailableRooms] = useState<{ name: string; displayName: string }[]>([]);
  // activeChannel is now derived, not stored in state
type RoomRequestFlagAction =
  | { type: "push"; flag: "pause" | "ask"; displayName: string; name: string }
  | { type: "pop" }
  | { type: "reset" };

function roomRequestFlagReducer(
  state: RoomRequestFlagStack,
  action: RoomRequestFlagAction
): RoomRequestFlagStack {
  switch (action.type) {
    case "push":
      return [
        ...state,
        { flag: action.flag, displayName: action.displayName, name: action.name }
      ];
    case "pop":
      return state.length > 0 ? state.slice(1) : state;
    case "reset":
      return [];
    default:
      return state;
  }
}

const [requestRoomChangeFlagStack, dispatchRequestRoomChangeFlag] = useReducer(
  roomRequestFlagReducer,
  []
);
const requestRoomChangeFlag = requestRoomChangeFlagStack[0] || { flag: "", displayName: "", name: "" };
  const [requestRestartFlag, setRequestRestartFlag] = useState<boolean>(false);
  const [didRoomChange, setDidRoomChange] = useState<RoomChangePrompt>('noSignal');
 
  // Get the subroom change listener and getMessages function from the WebSocket context.
  const { requestNewRoomListener, getMessages, requestRoomChange} = useWebSocketContext();

  // When availableRoomNames are set (or updated) and no subroom is selected yet,
  // default to the first subroom (index 0).
  // When availableRoomNames are set (or updated) and no subroom is selected yet,
  // default to the first subroom (index 0).
  useEffect(() => {
    if (availableRooms.length > 0 && activeChatRoomName === '') {
      setActiveChatSubRoomName(availableRooms[0].name);
    }
  }, [activeChatRoomName, availableRooms]);
  
  // Listen for subroom change messages.
  // Listen for subroom change messages.
  useEffect(() => {
    requestNewRoomListener((msg: WebSocketNewRoomMessage) => {
      const match = availableRooms.find(r => r.name === msg.To);
      if (msg.SubAction === "change-room") {
        dispatchRequestRoomChangeFlag({
          type: "push",
          flag: "pause",
          displayName: match ? match.displayName : msg.To,
          name: msg.To
        });
      } else {
        console.log("Room change request received");
        dispatchRequestRoomChangeFlag({
          type: "push",
          flag: "ask",
          displayName: match ? match.displayName : msg.To,
          name: msg.To
        });
      }
    });
  }, [requestNewRoomListener, availableRooms]);

  // Handle room change prompt state
  useEffect(() => {
    if (didRoomChange === "changeRoom") {
      const pendingRoom = requestRoomChangeFlag.name;
      console.log("Changing room to:", pendingRoom);
      setActiveChatSubRoomName(pendingRoom);

      // TODO: Replace 'userid' with actual user ID from context/props
      if (requestRoomChangeFlag.flag === "ask") {
        requestRoomChange("user", activeChatRoomName, pendingRoom);
      }
      // No need to setActiveChannel, now derived

      // Clear pending request after processing the change
      dispatchRequestRoomChangeFlag({ type: "pop" });
      setRequestRestartFlag(false);
      setDidRoomChange("noSignal");
    } else if (didRoomChange === "noChange") {
      console.log("Room change declined; staying in:", activeChatSubRoomName);
      // Clear pending request after processing the change
      dispatchRequestRoomChangeFlag({ type: "pop" });
      setRequestRestartFlag(false);
      setDidRoomChange("noSignal");
    }
  }, [didRoomChange, availableRooms, activeChatSubRoomName, requestRoomChangeFlag.flag, requestRoomChange, activeChatRoomName, requestRoomChangeFlag.name]);

  // Function to retrieve messages for the active channel.
  // Derive activeChannel from availableRooms and activeChatSubRoomName
  const activeChannel = availableRooms.findIndex(r => r.name === activeChatSubRoomName);

  // Function to retrieve messages for the active channel.
  const getMessagesForChannel = (): WebSocketReplyChatRoomMessage[] => {
    const messages = getMessages(activeChatRoomName);
    return messages.filter(
      (msg) =>
        msg.RoomName === activeChatSubRoomName &&
        (msg.SubAction === 'reply' || msg.SubAction === 'ask')
    );
  };

  // Only expose necessary values in context
  return (
    <AppStateContext.Provider
      value={{
        activeChatRoomName,
        setActiveChatRoomName,
        activeChatSubRoomName,
        setActiveChatSubRoomName,
        availableRooms,
        setAvailableRooms,
        activeChannel,
        getMessagesForChannel,
        requestRoomChange: requestRoomChangeFlag,
        requestRestart: requestRestartFlag,
        setDidRoomChange,
      }}
    >
      {children}
    </AppStateContext.Provider>
  );
};

/**
 * Custom hook for accessing AppStateContext.
 * Throws if used outside AppStateProvider.
 */
export const useAppStateContext = (): AppStateContextProps => {
  const context = useContext(AppStateContext);
  if (!context) {
    throw new Error('useAppStateContext must be used within an AppStateProvider');
  }
  return context;
};
