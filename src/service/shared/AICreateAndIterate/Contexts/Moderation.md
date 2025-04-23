# Moderation

A `moderation` block defines rules and prompts for moderating conversations within a room or agent context. It is used to enforce content policies, filter inappropriate messages, and guide agent behavior according to moderation requirements.

## Purpose

- **Content filtering:** Prevent inappropriate or undesired content.
- **Policy enforcement:** Ensure conversations adhere to defined guidelines.
- **Guidance:** Provide prompts or instructions for moderation actions.

## Format

- The `moderation` block must be an object.
- It may include prompts, rules, or references to moderation logic.
- Moderation prompts must not be empty.
- Only one moderation block should be defined per context (room or agent).

**Not allowed:**
- Empty moderation blocks or prompts.
- Multiple moderation blocks in the same context.
- Non-object moderation definitions.

## Example

```yaml
rooms:
  - name: "MainRoom"
    agents:
      - name: "AgentA"
        instructions: "..."
        moderation:
          prompt: "Do not allow offensive language."
```

## Best Practices

- Clearly define moderation prompts and rules.
- Avoid empty or ambiguous moderation instructions.
- Place moderation blocks where content filtering is required.

Proper moderation configuration ensures safe, policy-compliant, and robust multi-agent chat flows.
