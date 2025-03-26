import React from 'react';
import './IntroPage.css';

const IntroPage: React.FC = () => {
  return (
    <div className="intro-page-container">
      <h1>Agents and Orchestrators in Multi-Agent Systems</h1>
      <p>
        In today’s advanced AI landscape, coordinating multiple specialized units is essential for building scalable, efficient, and robust applications.
        This document introduces two fundamental components—<strong>Agents</strong> and <strong>Orchestrators</strong>—and explains how various orchestration patterns can be applied to real-world tasks.
      </p>

      <section>
        <h2>What is Semantic Kernel?</h2>
        <p>
          <a href="https://github.com/microsoft/semantic-kernel">Semantic Kernel (SK)</a> is Microsoft's open-source toolkit that integrates large language models into conventional programming environments, simplifying the creation of multi-agent systems with built-in memory management, vector store connectors, and flexible orchestration features.
        </p>
      </section>

      <section>
        <h2>Agents</h2>
        <p>
          An <strong>Agent</strong> is a specialized, self-contained unit designed to perform particular tasks within a larger system. Examples include:
        </p>
        <ul>
          <li><strong>Chat Agents:</strong> Conversational response generation.</li>
          <li><strong>Information Gathering Agents:</strong> External data retrieval.</li>
          <li><strong>Grounding Agents:</strong> Data-supported response generation.</li>
          <li><strong>System Integration Agents:</strong> Interaction with external devices and systems.</li>
        </ul>
      </section>

      <section>
        <h2>Orchestrators</h2>
        <p>
          An <strong>Orchestrator</strong> is a central controller coordinating interactions among multiple agents to complete complex tasks. Orchestrators handle:
        </p>
        <ul>
          <li>Agent selection and task delegation.</li>
          <li>Maintaining coherent communication flows.</li>
          <li>Implementing termination and refinement strategies.</li>
        </ul>
      </section>

      <section>
        <h2>Orchestration Patterns</h2>
        <p>
          Multi-agent systems leverage various orchestration patterns, including:
        </p>
        <ul>
          <li><strong>Agent Chat Orchestration:</strong> Sequential invocation for iterative refinement.</li>
          <li><strong>Parallel Orchestration:</strong> Concurrent agent activation for rapid responses.</li>
          <li><strong>Collaboration Orchestration:</strong> Structured responses combined with asynchronous feedback.</li>
          <li><strong>Hierarchical Orchestration:</strong> Multi-layer delegation among orchestrators and agents for complex tasks.</li>
        </ul>
      </section>

      <section>
        <h2>Sample Scenarios</h2>
        <p>
          Detailed practical scenarios illustrate how each orchestration pattern can be applied:
        </p>
        <ul>
          <li>Reflexion Agent Chat</li>
          <li>Crisis Management Agent Chat</li>
          <li>Adaptive Collaborative Control</li>
          <li>Military Team Agent Chat</li>
          <li>And many more...</li>
        </ul>
      </section>

      <section>
        <h2>Conclusion</h2>
        <p>
          Leveraging Agents and Orchestrators with orchestration patterns allows for the development of adaptive, robust, and scalable multi-agent AI systems. Explore these techniques and enhance your system designs using Semantic Kernel.
        </p>
      </section>
    </div>
  );
};

export default IntroPage;
