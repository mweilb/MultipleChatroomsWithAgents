
# Overview
This prototype handles agent-based interactions in a conversational environment by integrating multiple AI agents. These agents work together to respond to user queries, execute tasks, and interact with one another.  

For more details on Semantic Kernel, Agents, and Orchestrators, please see the [Deeper on Semantic Kernel, Agents and Orchestrators](./docs/overall.md) document.

## Core Features 

- **Multi-Agent Chat Room:** ✅ 
  A flexible chat environment where various agents interact with each other and the user. The system provides clear visibility into ongoing processes, enabling rapid iteration and efficient learning. It is designed not as a final product interface, but as a tool for those creating the experience

  <img src="./docs/media/action.gif" alt="Demo GIF" style="width:40%">

- **YAML Configuration:**  
  Each chat room is defined via a YAML configuration that specifies agent roles, instructions, selection criteria, termination conditions, and more.
    - **[Yaml Overview ✅](./docs/yamlNext.md):** 
-   - **[Yaml Samples ✅](./docs/yaml.md#samples):** 
- **Flexible Orchestration:**  
  Supports multiple orchestration strategies to manage how agents interact. In our prototype, we’ve implemented custom orchestrations that include:
  - **[Agent Chat Orchestration (Sequential Flow) ✅](./docs/overall.md#agent-chat-orchestration):**  
    The orchestrator sequentially triggers agents one after the other, evaluating each output before moving on. This approach is ideal for iterative refinement.
  - **[Parallel Orchestration (🚧)](./docs/overall.md#parallel-orchestration):**  
    Multiple agents are activated concurrently, with their responses aggregated by the orchestrator. This reduces latency and offers redundancy.
  - **[Collaboration Orchestration (🚧)](./docs/overall.md#collaboration-orchestration):**  
    A hybrid strategy where a primary orchestrator handles structured inputs and a secondary orchestrator provides asynchronous, creative feedback.
  - **[Hierarchical Orchestrator-to-Orchestrator (🚧)](./docs/overall.md#hierarchical-orchestrator-to-orchestrator):**  
    A layered approach in which a primary orchestrator delegates tasks to subordinate orchestrators, each managing a group of specialized agents. Their aggregated outputs are then combined into a final response.

- **Service Integration:**  
  Easily switch between Ollama, Azure, or OpenAI, adapting to different deployment environments.

 



## Local Run Instructions
Refer to the guides for your operating system:
- **Windows**: [Local Run - winOS.md](./docs/winOs.md)
- **macOS**: [Local Run - macOs.md](./docs/macOs.md)

## Demo Overview
For a quick look at the system in action, check out the demo using DeepSeek locally. The demo video includes a brief initial pause to accommodate real-time processing.

<img src="./docs/media/action.gif" alt="Demo GIF" style="width:80%">

## Screenshots
Here are some screenshots that showcase various aspects of the prototype:

- **YAML Setup**  
  Configure the system using code or YAML.  
  <img src="./docs/media/Yml.png" alt="Yaml Format" style="width:60%">

- **Copywriter Interface**  
  This interface filters the chat history so that the language model focuses on a smaller subset of content.  
  <img src="./docs/media/CopyWriter.png" alt="Copywriter Interface" style="width:60%">

- **Art Director Thinking**  
  A visual depiction of the Art Director formulating the next steps.  
  <img src="./docs/media/ArtDirector-Thinking.png" alt="Art Director Thinking" style="width:60%">

- **Art Director Response**  
  The Art Director’s response, including an evaluation of the termination decision-making process.  
  <img src="./docs/media/ArtDirector.png" alt="Art Director Response" style="width:60%">

- **System Diagram with Mermaid**  
  An architectural diagram of the system, generated using Mermaid.  
  <img src="./docs/media/Mermaid.png" alt="Mermaid Diagram" style="width:60%">

 
