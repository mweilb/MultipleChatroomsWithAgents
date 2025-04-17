export interface WebSocketBaseMessage {
    UserId: string;
    TransactionId: string;
    Action: string;
    SubAction: string;
    Content: string;

    Mode: string;
  }
  