# Name Collision

A `name-collision` error occurs when two or more entities (rooms, agents, or rules) share the same name within a configuration. Unique naming is required to ensure unambiguous references and correct system behavior.

## Purpose

- **Uniqueness:** Guarantee that each room, agent, and rule can be uniquely identified.
- **Clarity:** Prevent confusion and errors caused by duplicate names.
- **Validation:** Enforce naming rules at configuration time.

## Format

- All room names must be unique.
- All agent names must be unique within their room and must not collide with room names.
- Rule names (if used) must be unique within their context.
- Names are case-sensitive.

**Not allowed:**
- Duplicate room names.
- Agent names that match any room name.
- Duplicate agent names within the same room.
- Duplicate rule names within the same context.

## Example

```yaml
rooms:
  - name: "MainRoom"
    agents:
      - name: "AgentA"
      - name: "AgentA"  # Not allowed: duplicate agent name
  - name: "MainRoom"    # Not allowed: duplicate room name
    agents:
      - name: "AgentB"
```

## Best Practices

- Use descriptive, unique names for all entities.
- Check for accidental reuse of names across rooms and agents.
- Consider using naming conventions to avoid collisions.

Proper naming prevents ambiguity and ensures robust, maintainable multi-agent chat configurations.
