# WebSocket Message Schema

This document describes the schema for messages exchanged over WebSocket between the frontend (TypeScript) and backend (C#).

## Base Message Structure

| Field         | Type    | Description                                      |
|---------------|---------|--------------------------------------------------|
| UserId        | string  | Identifier of the user                           |
| TransactionId | string  | Unique identifier for the message transaction    |
| Action        | string  | Primary action to be performed                   |
| SubAction     | string  | Sub-action associated with the primary action    |
| Content       | string  | Content or payload of the message                |
| Mode          | string  | Application mode (e.g., "App", "Editor")         |
| Version       | string  | Protocol version (e.g., "1.0")                   |

## Example (JSON)

```json
{
  "UserId": "user-123",
  "TransactionId": "txn-456",
  "Action": "chat",
  "SubAction": "reply",
  "Content": "Hello, world!",
  "Mode": "App",
  "Version": "1.0"
}
```

## Keeping Types in Sync

To avoid duplication and ensure consistency between TypeScript and C#:

- **Option 1: quicktype**
  - Use [quicktype](https://quicktype.io/) to generate TypeScript and C# types from a JSON schema or example.
  - Update the schema here and regenerate types as needed.

- **Option 2: OpenAPI/Swagger**
  - Define message schemas in an OpenAPI YAML/JSON file.
  - Use code generation tools (e.g., NSwag, openapi-generator) to generate both TypeScript and C# types.

## Workflow Recommendation

1. Update this schema documentation when message fields change.
2. Use quicktype or OpenAPI to generate/update types in both codebases.
3. Review and test for compatibility after changes.
