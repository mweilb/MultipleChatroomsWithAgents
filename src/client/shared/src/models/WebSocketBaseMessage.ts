export interface WebSocketBaseMessage {
    UserId: string;
    TransactionId: string;
    Action: string;
    SubAction: string;
    Content: string;
    RoomName: string;
    SubRoomName: string;
    Hints: { [key: string]: { [key: string]: string } };

  }
  