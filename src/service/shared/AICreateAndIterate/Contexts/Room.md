# Room

A `room` defines a logical grouping of agents and rules within a multi-agent chat configuration. Each room controls its own participants and flow, enabling modular and organized conversation structures.

## Purpose

- **Organization:** Group agents and rules for specific conversation contexts.
- **Scoping:** Limit agent actions and rules to a defined context.
- **Flow control:** Enable transitions between different rooms for complex workflows.

## Format

- Each room must have a unique `name` (string).
- A room must define at least one agent.
- Rooms may include rules, termination blocks, and other configuration options.
- Room names must not collide with other room or agent names.
- The room definition must be an object, not a list or primitive.

**Not allowed:**
- Duplicate room names.
- Rooms without agents.
- Non-object room definitions.
- Using reserved keywords as room names.

## Example

```yaml
rooms:
  - name: "MainRoom"
    agents:
      - name: "AgentA"
        instructions: "..."
    rules:
      - ...
    termination:
      regex-termination:
        pattern: "exit"
  - name: "SupportRoom"
    agents:
      - name: "AgentB"
        instructions: "..."
```

## Best Practices

- Use descriptive, unique names for each room.
- Clearly define agents and rules within each room.
- Avoid name collisions with agents or other rooms.
- Keep room definitions modular for maintainability.

Proper room definitions ensure clear, maintainable, and robust multi-agent chat flows.
