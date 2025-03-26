# Introduction to the System

## Overview
This system is designed for handling agent-based interactions in a conversational environment, integrating multiple AI agents to respond to user queries, perform tasks, and interact with other agents. The system leverages **Ollama**, **Azure Cognitive Search**, and **Azure OpenAI** services to provide intelligent responses, while YAML-based agent configurations guide how agents respond.

## Core Features
- **Multi-Agent Chat Room**: A flexible chat room where different agents communicate with each other and the user.
- **Selection Strategy**: Determines which agent should respond next based on the conversation history and context.
- **Termination Strategy**: Controls when to stop the conversation based on a true/false question or predefined conditions.
- **YAML Configuration**: Each agent is defined using a YAML configuration, which includes instructions, selection criteria, termination conditions, and more.

## Technologies Used
1. **Ollama**: Local AI server for chat and text generation.
2. **Azure Cognitive Search**: Used for vector-based document search and semantic search capabilities.
3. **Azure OpenAI**: Provides AI-driven responses via GPT models for various tasks.
4. **YAML**: Used to configure agent instructions, selection criteria, and termination conditions.
-
