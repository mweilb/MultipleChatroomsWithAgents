// sectionDefinitions.ts

import { Section } from '../types/SectionTypes';

/**
 * The complete set of message section definitions.
 * Each section defines how data is structured and displayed, based on the message received from the WebSocket.
 * The keys and subkeys must match the corresponding messages from the WebSocket server.
 */
export const messageSectionDefinitions: Section[] = [
  {
    title: "Agent Selection", // Title of the section
    key: "SelectionRationale", // Unique identifier for this section
    mode: "group", // Defines this section as a grouped type
    groups: [ // Groups of related fields
      {
        name: "Select Messages", // Group name
        key: "select-message-selection", // Unique identifier for this group
        fields: [ // Fields within this group
          { key: "select-history", subkey: "prompt", label: "Prompt" },
          { key: "select-history", subkey: "reason", label: "Thinking" },
          { key: "select-history", subkey: "content", label: "Results" },
        ],
      },
      {
        name: "Select Agent", // Group name
        key: "select-agent", // Unique identifier for this group
        fields: [
          { key: "select-content", subkey: "prompt", label: "Prompt" },
          { key: "select-content", subkey: "reason", label: "Thinking" },
          { key: "select-content", subkey: "content", label: "Results" },
        ],
      },
    ],
  },
  {
    title: "Agent Processing", // Title of the section
    key: "AgentProcessing", // Unique identifier for the section
    fields: [ // Standalone fields
      { key: "agent", subkey: "reasons", label: "Thinking" },
    ],
  },
  {
    title: "Chat Termination", // Title of the section
    key: "terminate-rationale", // Unique identifier for the section
    mode: "group", // Defines this section as a grouped type
    groups: [ // Groups of related fields
      {
        name: "Select Messages", // Group name
        key: "select-message-terminate", // Unique identifier for this group
        fields: [
          { key: "terminate-history", subkey: "prompt", label: "Prompt" },
          { key: "terminate-history", subkey: "reason", label: "Thinking" },
          { key: "terminate-history", subkey: "content", label: "Results" },
        ],
      },
      {
        name: "Decide to Terminate", // Group name
        key: "decide-to-terminate", // Unique identifier for this group
        fields: [
          { key: "terminate-content", subkey: "prompt", label: "Prompt" },
          { key: "terminate-content", subkey: "reason", label: "Thinking" },
          { key: "terminate-content", subkey: "content", label: "Results" },
        ],
      },
    ],
  },
];
