# Round Robin Selection

The `round-robin-selection` block defines a selection strategy where agents or rules are activated in a repeating, cyclical order. It is used to distribute actions evenly among participants in multi-agent chat configurations.

## Purpose

- **Fairness:** Ensure each agent or rule gets an equal opportunity to act.
- **Load balancing:** Distribute work or responses evenly.
- **Declarative configuration:** Make the selection logic explicit and maintainable.

## Format

- The `round-robin-selection` block must be an object.
- It must include an `agents` or `rules` field listing the entities to cycle through.
- Only one `round-robin-selection` block should be present per context.
- The list must be non-empty and reference valid agent or rule names.

**Not allowed:**
- Multiple `round-robin-selection` blocks in the same context.
- Empty or invalid lists.
- Non-object `round-robin-selection` definitions.
- Referencing non-existent agents or rules.

## Example

```yaml
selection:
  round-robin-selection:
    agents: ["AgentA", "AgentB", "AgentC"]
```

## Best Practices

- Use round-robin-selection for scenarios requiring balanced participation.
- Ensure all referenced agents or rules exist and are spelled correctly.
- Document the intended cycle for maintainability.

Proper use of `round-robin-selection` enables fair, predictable control over conversation flow.
