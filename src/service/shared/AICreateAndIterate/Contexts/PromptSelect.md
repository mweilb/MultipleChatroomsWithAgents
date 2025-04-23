# Prompt Select

The `prompt-select` block defines a selection mechanism based on user or agent prompts. It is used to choose the next agent, rule, or action by matching a specific prompt or message.

## Purpose

- **Prompt-based selection:** Activate agents or rules based on specific prompts.
- **Dynamic control:** Enable flexible, context-sensitive selection logic.
- **Declarative configuration:** Make selection logic explicit and maintainable.

## Format

- The `prompt-select` block must be an object.
- It must include a `prompt` or similar field specifying the message to match.
- Only one `prompt-select` block should be present per context.
- The prompt value must be non-empty and relevant.

**Not allowed:**
- Multiple `prompt-select` blocks in the same context.
- Empty or missing prompt values.
- Non-object `prompt-select` definitions.

## Example

```yaml
selection:
  prompt-select:
    prompt: "Choose an option"
```

## Best Practices

- Use prompt-select for clear, actionable selection triggers.
- Ensure the prompt value is unique and unlikely to be triggered accidentally.
- Document the purpose of the prompt for maintainability.

Proper use of `prompt-select` enables flexible, maintainable selection logic in multi-agent chat flows.
