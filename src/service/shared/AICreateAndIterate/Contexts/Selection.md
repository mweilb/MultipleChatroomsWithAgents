# Selection

A `selection` block defines how agents or rules are chosen to participate or act within a room. It controls the logic for selecting the next agent, rule, or action, supporting various strategies for multi-agent workflows.

## Purpose

- **Control flow:** Determine which agent or rule is activated next.
- **Strategy:** Support different selection mechanisms (e.g., round-robin, sequential, prompt-based).
- **Flexibility:** Enable dynamic or static selection logic.

## Format

- The `selection` block must be an object.
- Only one selection strategy should be specified per block (e.g., `round-robin-selection`, `sequential-selection`, `prompt-select`).
- The selection strategy must be valid and supported by the system.
- Additional configuration may be required depending on the strategy.

**Not allowed:**
- Multiple selection strategies in the same block.
- Empty or non-object selection blocks.
- Unsupported or misspelled selection strategies.

## Example

```yaml
selection:
  round-robin-selection:
    agents: ["AgentA", "AgentB"]
```

## Best Practices

- Choose the selection strategy that best fits your workflow.
- Clearly specify required parameters for the chosen strategy.
- Avoid combining multiple strategies in a single block.

Proper selection configuration ensures predictable and maintainable agent or rule activation in multi-agent chat flows.
