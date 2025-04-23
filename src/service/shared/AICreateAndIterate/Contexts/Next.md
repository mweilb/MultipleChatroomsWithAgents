# Next

The `next` field specifies the agent or room that should be activated after the current step in a multi-agent chat configuration. It is used to control the flow of conversation and manage transitions between entities.

## Purpose

- **Flow control:** Define the next agent or room to activate.
- **Transition:** Enable explicit handoffs in the conversation.
- **Validation:** Ensure all transitions reference valid entities.

## Format

- The `next` field must reference a valid agent or room name.
- Only one `next` field should be present in a given context.
- The referenced name must exist in the configuration.
- The value must be a string.

**Not allowed:**
- Multiple `next` fields in the same context.
- Referencing a non-existent agent or room.
- Using non-string values.

## Example

```yaml
rules:
  - pattern: "help"
    action: "switch"
    next: "SupportRoom"
```

## Best Practices

- Ensure the `next` field always references a valid, defined agent or room.
- Use the `next` field to make transitions explicit and predictable.
- Avoid ambiguity by keeping transitions clear and direct.

Proper use of the `next` field enables robust, maintainable conversation flow in multi-agent chat systems.
