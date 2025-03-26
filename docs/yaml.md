# YAML Configurations for a Multi-Agent Chat Room

## Introduction

This document explains how YAML is used to configure a multi-agent chat room. We begin by introducing YAML and its benefits for our system, then break down the key syntax elements. Next, we describe how agents have access to the full conversation history and why we use both preset-conditions and filters to narrow that history for decision-making. Finally, we provide two sample configurations—one for a termination strategy and one for a selection strategy—with detailed explanations.

---

## What Is YAML?

YAML (which stands for "YAML Ain't Markup Language") is a human-readable data serialization format widely used for configuration files. Its simple, indentation-based structure makes it easy to read and edit, allowing you to define complex configurations in a straightforward manner.

---

## Why Use YAML for a Multi-Agent Chat Room?

Our multi-agent system uses YAML to configure:
- **Agent Roles & Behaviors:** Define each agent’s specific tasks and instructions.
- **Orchestration Rules:** Establish strategies for managing conversation flow (e.g., when to terminate a conversation or which agent should respond next).
- **Modularity & Flexibility:** Each configuration can be stored in its own file, allowing for rapid iteration and easy adjustments.

Using YAML ensures that configurations are both readable and flexible, simplifying the process of updating roles, instructions, and decision-making logic.

---

## Understanding the YAML Syntax

Our YAML configurations follow a consistent structure:

### 1. Metadata for the Chat Room

Each configuration begins with basic details such as a descriptive name and an emoji for visual identification.

```yaml
name: Copy and Brand Agents
emoji: 💬
```

### 2. Agents

Under the `agents` section, each agent is defined with:
- **Instructions:** A prompt on how the agent should respond to the message history.
- **Emoji:** A visual icon representing the agent.
- **Libraries (Optional):** A way to eventually integrate additional functionalities (for example, access to a vector store for grounding).

Every agent has access to the complete conversation history. We will decide later if we need to use the same feature for filters and preset-conditions (see strategies for more details).

**Example:**

```yaml
agents:
  ArtDirector:
    instructions: >
      You are an art director with a keen eye for copy. Your task is to review the copy,
      provide feedback, and conclude with **Approved** or **Not approved** to signal completion.
    emoji: 🎨
```

### 3. Strategies

Strategies govern the conversation flow and are divided into two types:
- **Termination:** Conditions under which the conversation should end (e.g., when a final decision is reached).
- **Selection:** Rules to decide which agent responds next (e.g., alternating between agents).

Each strategy includes:
- **description:** A prompt that explains how to implement the strategy.

And the ability to reduce the message history to something meaningful before the description prompt is applied:
- **preset-conditions:** Predefined criteria to manipulate the full message history, such as:
  - "Last message" – only provide the last message in the history.
  - "Remove content" – remove the content of the message, leaving only the name.
  - More actions may be added as needed.
- **filters:** A prompt that narrows down the full message history to only the most relevant parts for evaluation.

Preset-conditions and filters can be used together for precise context or separately for simpler evaluations.

**Example for a termination strategy:**

```yaml
strategies:
  termination:
    description: >
      Terminate the conversation when:
        - The responding agent is ArtDirector.
        - The final sentence of the message is either **Approved** or **Not approved**.
    preset-conditions: ["Last message"]
    filter: >
      Narrow the message history to only the most recent message for evaluation.
```

**Example for a selection strategy:**

```yaml
strategies:
  selection:
    description: >
      Selects the responding name based on the following rules:
        - If the name field is "CopyWriter", choose "ArtDirector".
        - If the name field is "ArtDirector", choose "CopyWriter".
        - Otherwise, choose "CopyWriter".
    preset-conditions: ["Last message", "Remove content"]
```

<h2 id="samples">Sample Configurations</h2>
 
Below are two YAML examples that demonstrate similar syntax for a termination strategy and a selection strategy, with detailed explanations for clarity.

### Example 1: Copy and Brand  

This configuration terminates the conversation when the ArtDirector’s feedback concludes with **Approved** or **Not approved**. The filter ensures that only the most recent message is evaluated.

```yaml
name: Copy and Brand Agents - Termination Strategy
emoji: 💬

agents:
  ArtDirector:
    instructions: >
      You are an art director with a keen eye for copy. Your task is to review the copy,
      provide feedback, and conclude your message with **Approved** or **Not approved** to signal completion.
      Explain briefly why you feel that way.
    emoji: 🎨

  CopyWriter:
    instructions: >
      You are a seasoned copywriter known for brevity and dry humor. Your task is to refine
      the copy into a single, concise proposal.
    emoji: ✍️

strategies:
  termination:
    description: >
      Terminate the conversation when:
        - The responding agent is ArtDirector.
        - The final sentence of the message is either **Approved** or **Not approved**.
    preset-conditions: ["Last message"]
    filter: >
      Narrow the message history to only the most recent message for evaluation.
```

**Explanation:**  
- **Agents Section:**  
  The ArtDirector provides the final feedback, while the CopyWriter refines the copy.
- **Termination Strategy:**  
  The conversation ends when the ArtDirector’s latest message concludes with a definitive verdict (**Approved** or **Not approved**).  
  - *Preset-conditions* ensure that only the last message is considered.
  - *Filter* isolates that message to streamline the evaluation process.

---

### Example 2: Architecture Design  

This configuration alternates responses among multiple roles (e.g., ProgramManager, ProductOwner, TechnicalManager, Architect, TechnicalWriter) based on the sender of the last message. A filter isolates the latest message, ensuring that the selection decision is based solely on its sender.

```yaml
name: ArchitectureDesign
emoji: 🏗️

agents:
  ProgramManager:
    emoji: 📊
    instructions: >
      Your role is to summarize current progress and identify next steps.
      Evaluate:
        - Whether all requirements are clear.
        - If the ProductOwner is satisfied.
        - If the TechnicalManager agrees.
        - Whether the TechnicalWriter has produced a high-level document and diagram.
      Always list requirements and next steps with clear assignments.
      Example:
        Requirements:
          1. Requirement 1
          2. Requirement 2
          3. Requirement 3
        Next Steps:
          1. ProductOwner to provide missing details.
          2. TechnicalManager to coordinate questions with the Architect and ProductOwner.
          3. TechnicalWriter to create and clean up the diagram.

  ProductOwner:
    emoji: 🛠️
    instructions: >
      Your role is to fill in missing details and clarify open questions.
      You are the expert on requirements and desired outcomes.

  TechnicalManager:
    emoji: 🧠
    instructions: >
      Your role is to ensure the plan is technically feasible and meets requirements.
      Maintain alignment with the Architect.

  Architect:
    emoji: 🏛️
    instructions: >
      You are responsible for designing the software architecture based on requirements.
      If requirements are unclear, ask for clarification; otherwise, create a high-level architecture diagram in Mermaid format.
      Collaborate with the ProductOwner and Client as needed.

  TechnicalWriter:
    emoji: ✍️
    instructions: >
      Clean up and validate the architecture diagram.
      Ensure the Mermaid format is correct and presentable.

strategies:
  termination:
    description: >
      Terminate the conversation when:
        - The responding agent is ProgramManager.
        - There are no next steps left.
    preset-conditions: ["Last message"]

  selection:
    description: >
      Select the next agent based on the following rules:
        - If the last message was from ProgramManager, identify the first pending step and choose the corresponding agent.
        - Otherwise, alternate among agents based on their roles (select only from {ProgramManager, ProductOwner, TechnicalManager, Architect, TechnicalWriter}).
    preset-conditions: ["Last message"]
```

**Explanation:**  
- **Agents Section:**  
  This configuration includes multiple roles that cover the full architecture design process:
  - **ProgramManager:** Provides an overview and outlines next steps.
  - **ProductOwner:** Fills in missing details.
  - **TechnicalManager:** Ensures technical feasibility.
  - **Architect:** Designs the architecture.
  - **TechnicalWriter:** Cleans up the output.
- **Termination Strategy:**  
  The conversation terminates when the ProgramManager confirms that there are no further steps required.
- **Selection Strategy:**  
  The next agent is chosen based on the sender of the last message. For example, if the ProgramManager’s response is last, the system examines the pending tasks to determine which agent should respond next.
  - *Preset-conditions* ensure that only the most recent message is used for this decision, ensuring an orderly flow.

---

## Conclusion

This document has demonstrated how YAML is used to configure a multi-agent chat room by defining agents, establishing orchestration strategies, and refining the conversation history using preset-conditions and filters. The sample configurations for termination and selection strategies illustrate these concepts in practice, ensuring precise and effective decision-making in the conversation flow.

 