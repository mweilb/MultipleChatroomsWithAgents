Certainly! Here’s the **README 3: Usage Guide** focusing on the **YAML Configuration for Agents** section.

---

# **README-Usage.md: Usage Guide**

This document explains how to use the system once it has been set up. It covers how to interact with agents using **Selection Strategy**, **Termination Strategy**, and **YAML configuration**.

---

## **1. Termination Strategy**

The **Termination Strategy** determines when to stop the conversation. It works by asking a **True/False** question. When the agent answers this question, the system evaluates the response to decide whether to continue or end the conversation.

### **1.1 YAML Example for Termination Strategy:**
```yaml
agent_name: "ApprovalBot"
emoji: "✔️"
instructions: "Review the final product and decide whether it's acceptable. Respond with 'approved' or 'rejected'."
termination_conditions:
  - "If the agent says 'approved' or 'rejected', the conversation ends."
```

- **How It Works**: The **Termination Strategy** waits for the agent’s response. If the agent responds with **"approved"** or **"rejected"**, the conversation ends.

---

## **2. Selection Strategy**

The **Selection Strategy** determines **which agent should respond next** in the conversation. It evaluates the conversation history and selects the agent best suited to continue the dialogue based on the current context.

### **2.1 YAML Example for Selection Strategy:**
```yaml
agent_name: "MathSolver"
emoji: "🔢"
instructions: "Solve mathematical equations and provide step-by-step explanations."

selection_criteria:
  - "If the user asks a math-related question, select MathSolver."
  - "If the user asks about general queries, select GeneralAssistant."
```

- **How It Works**:
  - The **Selection Strategy** looks at the conversation history and decides which agent should respond.
  - If the user asks a math-related question, the **MathSolver** agent is selected.
  - If the question is general, the **GeneralAssistant** agent is chosen instead.

---

## **3. Filters and Pre-Conditions**

The **Filter** strategy reduces the conversation history to the most relevant parts, ensuring faster decision-making. **Pre-Conditions** are used for quick checks and actions before involving agents or invoking LLMs for processing.

### **3.1 Filter Example**

The **Filter** ensures only the most relevant parts of the conversation are passed along for further processing.

```yaml
filter:
  - "Only include the last 3 messages for decision making."
  - "Exclude messages that are unrelated to the current task."
```

- **How It Works**: The **Filter** narrows the history down, so only the last few messages are considered when selecting the next agent or making decisions. This helps speed up the process and ensures the system is focused on the latest context.

### **3.2 Pre-Conditions Example**

**Pre-Conditions** allow the system to quickly perform actions without invoking complex logic. These are predefined rules or checks that can be quickly applied.

```yaml
pre_conditions:
  - "If the user asks a yes/no question, immediately proceed with termination."
  - "If the user types 'exit', stop the conversation."
```

- **How It Works**: 
  - **Pre-Conditions** are faster than invoking the agent or using the LLM. If the user asks a **yes/no** question, the system will immediately evaluate it and proceed with termination. 
  - If the user types **"exit"**, the system terminates the conversation immediately without checking the entire conversation history or invoking the LLM.

---

## **4. Workflow Example**

### **4.1 Conversation Flow**

1. The **Selection Strategy** decides that the next agent should be **MathSolver** based on the user's question about mathematics.
2. **MathSolver** responds with a solution to the query.
3. The **Termination Strategy** asks if the user is satisfied with the solution. If the user responds with **"yes"**, the conversation ends.

### **4.2 Handling Multiple Agents**

- The system supports multiple agents, and the **Selection Strategy** will evaluate the context and select the most appropriate agent based on the conversation history.
- If the user’s query shifts, for example, from math to general queries, the **Selection Strategy** will change the selected agent (e.g., from **MathSolver** to **GeneralAssistant**).

---

## **5. Managing Multiple Agents with YAML**

The **YAML Configuration** allows you to define each agent’s behavior, the conditions for selecting that agent, and when to terminate the conversation. Below is an example that demonstrates how to handle multiple agents:

### **5.1 YAML Example with Multiple Agents**

```yaml
agent_name: "MathBot"
emoji: "🔢"
instructions: "Solve mathematical equations and provide step-by-step explanations."

selection_criteria:
  - "If the user asks a math-related question, select MathBot."

---

agent_name: "GeneralAssistant"
emoji: "🤖"
instructions: "Answer general queries and provide assistance with various tasks."

selection_criteria:
  - "If the user asks a general question, select GeneralAssistant."

---
```

- **How It Works**: 
  - The **Selection Strategy** evaluates the user’s query, and based on the context (math-related or general), it selects either **MathBot** or **GeneralAssistant**.
  - This flexibility allows the system to handle a wide range of interactions and queries, selecting the appropriate agent dynamically.

---

### Conclusion

This **Usage Guide** explains how to interact with the **YAML ChatRoom System** after it is set up. The **Termination Strategy** controls when the conversation stops based on True/False responses, while the **Selection Strategy** ensures the right agent is selected based on the conversation history and context. The **Filter** and **Pre-Conditions** make decision-making faster by reducing unnecessary history and applying quick, predefined checks.