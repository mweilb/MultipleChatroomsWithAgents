# Constant Termination

The `constant-termination` block defines a termination condition based on a fixed value or state. It is used to end a chat or rule when a specific, predefined condition is met.

## Purpose

- **Fixed termination:** End processing when a constant value or state is reached.
- **Simplicity:** Use straightforward, non-dynamic triggers for ending conversations or rules.
- **Declarative control:** Make termination logic explicit and easy to understand.

## Format

- The `constant-termination` block must be an object.
- It must include a `value` or similar field specifying the constant condition.
- Only one `constant-termination` block should be present per context.
- The value must be non-empty and valid for the intended logic.

**Not allowed:**
- Multiple `constant-termination` blocks in the same context.
- Empty or invalid constant values.
- Non-object `constant-termination` definitions.

## Example

```yaml
termination:
  constant-termination:
    value: true
```

## Best Practices

- Use constant-termination for simple, unambiguous end conditions.
- Clearly document what the constant value represents.
- Avoid using constant-termination for complex or dynamic scenarios.

Proper use of `constant-termination` enables clear, maintainable control over conversation flow.
