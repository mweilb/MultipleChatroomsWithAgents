# Regex Termination

The `regex-termination` block defines a termination condition based on matching a regular expression pattern in the conversation. It is used to end a chat or rule when specific text patterns are detected.

## Purpose

- **Pattern-based termination:** End processing when a message matches a regex.
- **Flexibility:** Support complex triggers for ending conversations or rules.
- **Declarative control:** Make termination logic explicit and maintainable.

## Format

- The `regex-termination` block must be an object.
- It must include a `pattern` field containing a valid regular expression string.
- Only one `regex-termination` block should be present per context.
- The pattern must be non-empty and syntactically valid.

**Not allowed:**
- Multiple `regex-termination` blocks in the same context.
- Empty or invalid regex patterns.
- Non-object `regex-termination` definitions.

## Example

```yaml
termination:
  regex-termination:
    pattern: "end of conversation"
```

## Best Practices

- Use clear, specific regex patterns to avoid unintended matches.
- Test regex patterns for correctness before deployment.
- Use regex-termination only when pattern-based control is required.

Proper use of `regex-termination` enables robust, flexible control over conversation flow.
