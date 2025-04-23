# Current

The `current` field refers to the agent or room that is actively processing or participating in the conversation at a given point in the workflow. It is used to track and manage the active context within multi-agent chat configurations.

## Purpose

- **State tracking:** Identify which agent or room is currently active.
- **Flow control:** Enable transitions and logic based on the current context.
- **Validation:** Ensure references to the current entity are valid and unambiguous.

## Format

- The `current` field must reference a valid agent or room name.
- Only one `current` field should be present in a given context.
- The referenced name must exist in the configuration.
- The value must be a string.

**Not allowed:**
- Multiple `current` fields in the same context.
- Referencing a non-existent agent or room.
- Using non-string values.

## Example

```yaml
current: "AgentA"
```

## Best Practices

- Ensure the `current` field always references a valid, defined agent or room.
- Use the `current` field to manage state transitions clearly.
- Avoid ambiguity by keeping the current context explicit.

Proper use of the `current` field enables robust state management and predictable conversation flow.
