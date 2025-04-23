# Sequential Selection

The `sequential-selection` block defines a selection strategy where agents or rules are activated in a fixed, sequential order. It is used to ensure a predictable, step-by-step flow in multi-agent chat configurations.

## Purpose

- **Order control:** Activate agents or rules in a specific sequence.
- **Predictability:** Ensure the same order is followed every time.
- **Declarative configuration:** Make the selection logic explicit and maintainable.

## Format

- The `sequential-selection` block must be an object.
- It must include an `agents` or `rules` field listing the entities in order.
- Only one `sequential-selection` block should be present per context.
- The list must be non-empty and reference valid agent or rule names.

**Not allowed:**
- Multiple `sequential-selection` blocks in the same context.
- Empty or invalid lists.
- Non-object `sequential-selection` definitions.
- Referencing non-existent agents or rules.

## Example

```yaml
selection:
  sequential-selection:
    agents: ["AgentA", "AgentB", "AgentC"]
```

## Best Practices

- Use sequential-selection for workflows that require strict ordering.
- Ensure all referenced agents or rules exist and are spelled correctly.
- Document the intended sequence for maintainability.

Proper use of `sequential-selection` enables clear, predictable control over conversation flow.
