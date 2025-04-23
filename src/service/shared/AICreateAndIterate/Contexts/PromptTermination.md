# Prompt Termination

The `prompt-termination` block defines a termination condition based on encountering a specific prompt or message in the conversation. It is used to end a chat or rule when a designated prompt is detected.

## Purpose

- **Prompt-based termination:** End processing when a specific prompt or message is encountered.
- **Declarative control:** Make termination logic explicit and easy to understand.
- **Flexibility:** Support scenarios where a particular message signals the end.

## Format

- The `prompt-termination` block must be an object.
- It must include a `prompt` or similar field specifying the message to match.
- Only one `prompt-termination` block should be present per context.
- The prompt value must be non-empty and relevant.

**Not allowed:**
- Multiple `prompt-termination` blocks in the same context.
- Empty or missing prompt values.
- Non-object `prompt-termination` definitions.

## Example

```yaml
termination:
  prompt-termination:
    prompt: "Session complete"
```

## Best Practices

- Use prompt-termination for clear, unambiguous end signals.
- Ensure the prompt value is unique and unlikely to be triggered accidentally.
- Document the purpose of the prompt for maintainability.

Proper use of `prompt-termination` enables clear, maintainable control over conversation flow.
