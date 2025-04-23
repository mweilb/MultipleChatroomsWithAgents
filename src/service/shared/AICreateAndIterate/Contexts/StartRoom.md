# Start Room

The `start-room` defines the entry point for a multi-agent chat configuration. It specifies which room the conversation begins in and is critical for correct initialization of the chat flow.

## Purpose

- **Initialization:** Clearly indicate where the conversation starts.
- **Control flow:** Ensure the system knows which room to activate first.
- **Validation:** Prevents ambiguous or missing entry points in the configuration.

## Format

- The `start-room` must reference a valid room name defined elsewhere in the configuration.
- Only one `start-room` should be specified per configuration.
- The referenced room must exist; referencing a non-existent room is not allowed.
- The value must be a string (the room name), not an object or list.

**Not allowed:**
- Multiple `start-room` entries.
- Referencing a room that is not defined.
- Using non-string values.

## Example

```yaml
start-room: "MainRoom"
rooms:
  - name: "MainRoom"
    agents: [...]
  - name: "SecondaryRoom"
    agents: [...]
```

## Best Practices

- Always define a `start-room` to avoid initialization errors.
- Ensure the referenced room exists and is spelled correctly.
- Use clear, descriptive room names for maintainability.

A valid `start-room` ensures predictable and robust initialization of multi-agent chat flows.
