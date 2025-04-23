# Instructions

The `instructions` field defines the behavior and guidance for an agent within a multi-agent chat configuration. Clear instructions are essential for agents to perform their roles effectively.

## Purpose

- **Guidance:** Specify what the agent should do or how it should behave.
- **Clarity:** Ensure agent actions are predictable and aligned with the system's goals.
- **Validation:** Prevent agents from operating without defined behavior.

## Format

- The `instructions` field must be present for every agent.
- Instructions must be a non-empty string.
- Instructions should be clear, actionable, and relevant to the agent's role.
- Only one instructions field per agent.

**Not allowed:**
- Missing or empty instructions fields.
- Non-string instructions values.
- Multiple instructions fields for a single agent.

## Example

```yaml
agents:
  - name: "AgentA"
    instructions: "Greet the user and answer questions."
  - name: "AgentB"
    instructions: "Provide technical support."
```

## Best Practices

- Write concise, specific instructions for each agent.
- Avoid vague or generic instructions.
- Tailor instructions to the agent's intended role.

Proper instructions ensure agents behave as expected and contribute to robust, maintainable chat flows.
