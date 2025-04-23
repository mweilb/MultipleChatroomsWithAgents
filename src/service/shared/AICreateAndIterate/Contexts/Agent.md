# Agent

An `agent` represents an autonomous participant in a multi-agent chat configuration. Agents are responsible for generating responses, following instructions, and interacting within rooms according to defined rules.

## Purpose

- **Participation:** Enable distinct conversational roles within a room.
- **Behavior:** Define instructions and logic for agent responses.
- **Modularity:** Allow flexible assignment of agents to different rooms.

## Format

- Each agent must have a unique `name` (string) within its room.
- Agents must include an `instructions` field describing their behavior.
- Agents may have additional configuration options (e.g., role, prompt, etc.).
- Agent names must not collide with other agent or room names.
- The agent definition must be an object, not a list or primitive.

**Not allowed:**
- Duplicate agent names within the same room or across rooms.
- Agents without instructions.
- Non-object agent definitions.
- Using reserved keywords as agent names.

## Example

```yaml
rooms:
  - name: "MainRoom"
    agents:
      - name: "AgentA"
        instructions: "Greet the user and answer questions."
      - name: "AgentB"
        instructions: "Provide support for technical issues."
```

## Best Practices

- Use descriptive, unique names for each agent.
- Write clear, actionable instructions for agent behavior.
- Avoid name collisions with rooms or other agents.
- Keep agent definitions modular for maintainability.

Proper agent definitions ensure clear roles and robust interactions in multi-agent chat flows.
