# YAML Syntax Guide for Multi-Room Chat Configurations

This document describes the valid YAML structure for defining one or more multi-room chat configurations. The YAML root is a mapping of configuration names to their respective chat room setups.

---

## Top-Level Structure

```yaml
<configuration_name_1>:
  # Valid fields at the top level of each configuration:
  emoji: "😀"                      # (Optional) Emoji for the configuration
  display-name: "Config Title"     # (Optional) Display name for the configuration
  name: "<config_name>"            # (Optional) Name of the configuration
  yield-on-room-change: "true"     # (Optional)
  yield-user-canceled: "cancel"    # (Optional)
  start-room: <room_name>          # (Optional) Name of the room to start in
  strategies:                      # (Optional) Strategies at the configuration level
    # (see strategies section below)
  moderation:                      # (Optional) Moderation at the configuration level
    # (see moderation section below)
  yield-on-room-change: "true"     # (Optional)
  yield-user-canceled: "cancel"    # (Optional)
  start-room: <room_name>          # (Optional) Name of the room to start in
  auto-start: <true|false>         # (Optional) Whether to auto-start (string, e.g. "true" or "false")
  chatrooms:                       # (Required) List of chat room definitions
    <room_name>:
      emoji: "😀"                 # (Optional) Emoji for the room
      display-name: "Room Title"  # (Optional) Display name for the room
      name: "<room_name>"         # (Required) Name of the room (should match the key)
      agents:                     # (Required) List of agents in the room
        - name: "<agent_name>"    # (Required) Name of the agent (no spaces or < | \ / >)
          # Optional agent settings:
          display-name: "Agent"
          emoji: "🤖"
          model: "gpt-3.5"
          instructions: "Be helpful."
          echo: "false"
          collection: "default"
      strategies:                 # (Optional) Room strategy settings
        rules:                    # List of rules for agent flow
          - name: "Rule Name"
            current:              # List of current agents
              - name: "agent1"
            selection:            # (Optional) Selection configuration
              sequential-selection:
                initial-agent: "agent1"
              round-robin-selection:
                initial-agent: "agent1"
                agents: ["agent1", "agent2"]
              prompt-select:
                instructions: "Choose the next agent."
                history-variable-name: "history"
                evaluate-name-only: "true"
                result-parser:
                  # See result parser section
                truncation-reducer:
                  target-count: "10"
                  threshold-count: "20"
                summarization-reducer:
                  target-count: "5"
                  threshold-count: "10"
            termination:          # (Optional) Termination configuration
              continuation-agent-name: "agent1"
              regex-termination:
                expressions:
                  - "pattern1"
                  - "pattern2"
              constant-termination:
                agents: ["agent1", "agent2"]
                value: "done"
              prompt-termination:
                agents: ["agent1"]
                instructions: "Summarize the conversation."
                history-variable-name: "history"
                result-parser:
                  regex:
                    - pattern: "Result: (.+)"
                      value: "$1"
                evaluate-name-only: "true"
                truncation-reducer:
                  target-count: "10"
                  threshold-count: "20"
                summarization-reducer:
                  target-count: "5"
                  threshold-count: "10"
            next:                 # (Optional) List of next agents
              - name: "agent2"
        termination:              # (Optional) Global termination config (same structure as above)
      moderation:                 # (Optional) Moderation settings
        # (custom fields, see below)
      yield-on-room-change: "true"    # (Optional) String flag for yielding on room change
      yield-user-canceled: "cancel"   # (Optional) String for user-canceled event

<configuration_name_2>:
  # ...another multi-room configuration...
```

---

## Field Details

- **<configuration_name>**: Top-level key for each configuration. No spaces or `< | \ / >`.
- **emoji, display-name, name, agents, strategies, moderation, yield-on-room-change, yield-user-canceled**: All valid at the configuration level, with the same meaning as in a room (see below).
- **start-room**: Name of the room to start in. Must match a key in `chatrooms`.
- **auto-start**: String value, e.g. `"true"` or `"false"`.
- **chatrooms**: Mapping where each key is a room name (no spaces or `< | \ / >`). Each value is a room definition.

### Room Definition

- All fields valid at the configuration level are also valid inside each room:
  - **emoji**: Emoji for the room (optional).
  - **display-name**: Human-readable name for the room (optional).
  - **name**: Room name (required, should match the key).
  - **agents**: List of agent objects. Each agent must have a `name` (no spaces or `< | \ / >`). Other fields are optional.
  - **strategies**: (optional) Defines agent flow and rules for the room.
    - **rules**: List of rules. Each rule can have:
    - **name**: Rule name (string)
      - **current**: List of current agents (each with a `name`)
      - **selection**: (optional) Selection configuration (choose one of the following):
        - **sequential-selection**:
          - **initial-agent**: (optional) Name of the initial agent
        - **round-robin-selection**:
          - **initial-agent**: (optional) Name of the initial agent
          - **agents**: (optional) List of agent names
        - **prompt-select**:
          - **instructions**: (optional) String
          - **history-variable-name**: (optional) String
          - **evaluate-name-only**: (optional) String
          - **result-parser**: (optional) See result parser section
          - **truncation-reducer**: (optional) See reducer section
          - **summarization-reducer**: (optional) See reducer section
      - **termination**: (optional) Termination configuration:
        - **continuation-agent-name**: (optional) Name of agent or room to continue to
        - **regex-termination**: (optional) End if any regex in `expressions` matches
          - **expressions**: List of regex patterns (strings)
        - **constant-termination**: (optional) End if value matches
          - **agents**: List of agent names (optional)
          - **value**: String value (optional)
        - **prompt-termination**: (optional) End based on prompt result
          - **agents**: List of agent names (optional)
          - **instructions**: String (optional)
          - **history-variable-name**: String (optional)
          - **result-parser**: (optional)
            - **regex**: List of regex expressions, each with:
              - **pattern**: Regex pattern (string)
              - **value**: Replacement value (string)
          - **evaluate-name-only**: String (optional)
          - **truncation-reducer**: (optional)
            - **target-count**: String (optional)
            - **threshold-count**: String (optional)
          - **summarization-reducer**: (optional)
            - **target-count**: String (optional)
            - **threshold-count**: String (optional)
      - **next**: (optional) List of next agents (each with a `name`)
    - **termination**: (optional) Global termination config (same structure as above)
  - **moderation**: (optional) Moderation rules (custom fields)
  - **yield-on-room-change**: String flag (optional)
  - **yield-user-canceled**: String for user-canceled event (optional)

### Agent Definition

- **name**: Required. No spaces or `< | \ / >`.
- **display-name**, **emoji**, **model**, **instructions**, **echo**, **collection**: Optional.

---

## Example

```yaml
main-config:
  start-room: main
  auto-start: "true"
  chatrooms:
    main:
      emoji: "🏠"
      display-name: "Main Room"
      name: "main"
      agents:
        - name: "assistant"
          display-name: "Helper Bot"
          emoji: "🤖"
          model: "gpt-3.5"
          instructions: "Assist users."
      strategies:
      moderation:
      yield-on-room-change: "true"
      yield-user-canceled: "cancel"
    support:
      emoji: "🛠️"
      display-name: "Support"
      name: "support"
      agents:
        - name: "support_agent"

test-config:
  start-room: support
  chatrooms:
    support:
      name: "support"
      agents:
        - name: "test_agent"
```

---

## Common Syntax Errors

- Indentation errors (YAML is indentation-sensitive).
- Configuration, room, or agent names with spaces or invalid characters (`<`, `|`, `\`, `/`, `>`).
- Missing required fields (`chatrooms`, `name` in each room, `name` in each agent).
- Mismatched `start-room` (must match a room key).

---
