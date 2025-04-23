# Introduction to Multi-Agent YAML Configuration Concepts

This documentation provides an overview of the key concepts and validation rules for defining multi-agent chat systems using YAML configuration files. The goal is to ensure robust, maintainable, and predictable chat flows by enforcing clear structure and validation for all configuration elements.

## Purpose of the File Format

The YAML file format enables you to declaratively define chat rooms, agents, rules, and control flow for multi-agent systems. By following these conventions, you ensure:

- **Clarity:** Each entity (room, agent, rule) is uniquely identified and well-defined.
- **Validation:** Errors such as name collisions, missing instructions, or invalid transitions are caught early.
- **Flexibility:** Supports advanced control flow, selection strategies, and termination conditions.
- **Maintainability:** Modular structure makes it easy to update, extend, and debug configurations.

## Rooms

Rooms are the primary building blocks of a multi-agent chat configuration. Each room defines a logical context for conversation, grouping together agents, rules, and control flow logic. Rooms enable modular design, allowing different parts of a conversation to be isolated, reused, or transitioned between as needed.

- **Organization:** Rooms group related agents and rules for specific tasks or conversation contexts.
- **Scoping:** Each room defines its own participants and logic, preventing unintended interactions.
- **Flow Control:** Transitions between rooms enable complex, multi-stage workflows.

A configuration typically starts in a designated "start room" and may transition between rooms based on rules, selections, or termination conditions.

## Key Concepts

Each concept below is documented in detail in its own markdown file:

- **[Start Room](src/service/shared/AICreateAndIterate/Contexts/StartRoom.md):** Defines the entry point for the chat flow.
- **[Room](src/service/shared/AICreateAndIterate/Contexts/Room.md):** Logical grouping of agents and rules.
- **[Agent](src/service/shared/AICreateAndIterate/Contexts/Agent.md):** Autonomous participant with instructions.
- **[Instructions](src/service/shared/AICreateAndIterate/Contexts/Instructions.md):** Behavioral guidance for agents.
- **[Rule](src/service/shared/AICreateAndIterate/Contexts/Rule.md):** Logic or triggers for agent/room actions.
- **[Current](src/service/shared/AICreateAndIterate/Contexts/Current.md):** Tracks the currently active agent or room.
- **[Next](src/service/shared/AICreateAndIterate/Contexts/Next.md):** Specifies the next agent or room to activate.
- **[Moderation](src/service/shared/AICreateAndIterate/Contexts/Moderation.md):** Content filtering and policy enforcement.
- **[Selection](src/service/shared/AICreateAndIterate/Contexts/Selection.md):** Mechanisms for choosing agents or rules.
  - **[Prompt Select](src/service/shared/AICreateAndIterate/Contexts/PromptSelect.md):** Selection based on prompts.
  - **[Sequential Selection](src/service/shared/AICreateAndIterate/Contexts/SequentialSelection.md):** Fixed order selection.
  - **[Round Robin Selection](src/service/shared/AICreateAndIterate/Contexts/RoundRobinSelection.md):** Cyclical, fair selection.
- **[Termination](src/service/shared/AICreateAndIterate/Contexts/Termination.md):** Defines when and how a chat or rule ends.
  - **[Regex Termination](src/service/shared/AICreateAndIterate/Contexts/RegexTermination.md):** Ends on regex match.
  - **[Constant Termination](src/service/shared/AICreateAndIterate/Contexts/ConstantTermination.md):** Ends on fixed value.
  - **[Prompt Termination](src/service/shared/AICreateAndIterate/Contexts/PromptTermination.md):** Ends on specific prompt.
- **[Name Collision](src/service/shared/AICreateAndIterate/Contexts/NameCollision.md):** Ensures all names are unique and unambiguous.

Refer to each linked document for allowed formats, restrictions, and best practices.

## Examples

### Single Room Example

```yaml
start-room: "MainRoom"
rooms:
  - name: "MainRoom"
    agents:
      - name: "AgentA"
        instructions: "Welcome the user and answer questions."
```

### Three Room Example

```yaml
start-room: "Lobby"
rooms:
  - name: "Lobby"
    agents:
      - name: "Greeter"
        instructions: "Greet users and direct them to the appropriate room."
    rules:
      - pattern: "support"
        action: "switch"
        next: "SupportRoom"
      - pattern: "sales"
        action: "switch"
        next: "SalesRoom"
  - name: "SupportRoom"
    agents:
      - name: "SupportAgent"
        instructions: "Help users with technical issues."
  - name: "SalesRoom"
    agents:
      - name: "SalesAgent"
        instructions: "Assist users with product information and purchases."
```

## Termination and Selection Example

Below is an example showing how to use both `termination` and `selection` blocks to control conversation flow:

```yaml
start-room: "MainRoom"
rooms:
  - name: "MainRoom"
    agents:
      - name: "AgentA"
        instructions: "Answer user questions."
      - name: "AgentB"
        instructions: "Provide additional support."
    selection:
      round-robin-selection:
        agents: ["AgentA", "AgentB"]
    termination:
      regex-termination:
        pattern: "goodbye|exit"
    rules:
      - pattern: "help"
        action: "switch"
        next: "SupportRoom"
  - name: "SupportRoom"
    agents:
      - name: "SupportAgent"
        instructions: "Assist users with technical issues."
```

- The `selection` block uses round-robin to alternate between AgentA and AgentB.
- The `termination` block ends the conversation in MainRoom if the user's message matches "goodbye" or "exit".
- Rules can trigger transitions to other rooms.

## Best Practices

- Use unique, descriptive names for all entities.
- Clearly define instructions and rules for each agent and room.
- Choose selection and termination strategies that fit your workflow.
- Validate your configuration to catch errors early.

Following these guidelines will help you build reliable, scalable, and maintainable multi-agent chat systems.
