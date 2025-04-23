# Rule

A `rule` defines a condition or logic that governs agent or room behavior within a multi-agent chat configuration. Rules enable advanced control over conversation flow and agent actions.

## Purpose

- **Logic:** Specify when and how agents or rooms should act.
- **Control:** Enforce constraints or triggers for actions.
- **Modularity:** Allow reusable, declarative logic blocks.

## Format

- Each rule should have a unique name within its context (if named).
- Rules must define a condition or trigger (e.g., pattern, event, or prompt).
- Rules may include actions, termination blocks, or references to agents.
- The rule definition must be an object.
- Rules must not be empty.

**Not allowed:**
- Duplicate rule names within the same context.
- Empty or non-object rule definitions.
- Missing required fields (e.g., condition or action).

## Example

```yaml
rules:
  - name: "EndOnGoodbye"
    pattern: "goodbye"
    action: "terminate"
  - pattern: "help"
    action: "switch-to-support"
```

## Best Practices

- Use descriptive, unique names for rules when naming is supported.
- Clearly define the trigger and action for each rule.
- Avoid overly complex or ambiguous rule logic.

Well-defined rules enable flexible, maintainable, and robust multi-agent chat flows.
