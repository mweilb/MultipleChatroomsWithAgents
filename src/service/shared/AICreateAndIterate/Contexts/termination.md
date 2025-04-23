# Termination

Termination defines when and how a chat room or agent's rule should end its processing and optionally hand off control to another agent or room. This mechanism enables flexible, declarative control over conversation flow, supporting scenarios like ending a session, switching agents, or responding to specific triggers.

## Purpose

- **Control flow:** Specify clear conditions for when a conversation or rule should stop.
- **Handover:** Optionally direct the next step by naming an agent or room to continue after termination.
- **Declarative triggers:** Use patterns, constants, or prompts to define termination in a readable, maintainable way.

## Format

A termination block can be added to a room or rule. Only one of the following keys may be present:

- `regex-termination`: Ends when a message matches a regular expression.
- `constant-termination`: Ends based on a fixed value or condition.
- `prompt-termination`: Ends when a specific prompt or message is encountered.

You may also specify:

- `continuation-agent-name` (optional): The agent or room to continue processing after termination. If not set, the system auto-generates a name (e.g., `<room_name>_Termination` or `<rule_name>_Termination`). This default may not correspond to any real agent or room unless you define it.

**Note:** If more than one termination type is specified, it is a configuration error.

## Example

```yaml
termination:
  regex-termination:
    pattern: "end of conversation"
  # Only one of the following should be present:
  # constant-termination:
  # prompt-termination:
  continuation-agent-name: "AgentB"
```

## Best Practices

- Use termination to make conversation flow explicit and maintainable.
- Always set `continuation-agent-name` to a valid agent or room if you want to control what happens next.
- Avoid specifying multiple termination types in the same block.

This approach enables modular, understandable, and robust control over multi-agent chat flows.
