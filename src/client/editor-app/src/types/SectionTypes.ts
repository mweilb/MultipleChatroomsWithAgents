// sectionTypes.ts

/**
 * Represents a single field within a message section.
 * A field is a piece of data with a unique key, a subkey (if applicable), 
 * and a label that is typically shown to users as the field's description.
 */
export interface Field {
  key: string;     // Unique identifier for the field
  subkey: string;  // Secondary identifier for the field (optional, can be used for more specific identification)
  label: string;   // Label for the field (e.g., the name displayed in the UI)
}

/**
 * Represents a group of related message fields.
 * A group organizes multiple fields under a single name to help structure the message section.
 */
export interface Group {
  name: string;    // The name of the group (used for display or identification)
  key: string;        // Optional key for the section, used for unique identification (e.g., "UserInfo")
  fields: Field[]; // A list of fields that belong to this group
}

/**
 * Represents an entire message section definition.
 * A section is a collection of fields, either grouped together or standalone.
 * It can also have an optional `mode` that defines the type of section it is ("ask" or "group").
 * The `parentKey` and `key` fields help identify the section for reference.
 */
export interface Section {
  title: string;       // The title of the message section (e.g., "User Information")
  key: string;        // Optional key for the section, used for unique identification (e.g., "UserInfo")
  groups?: Group[];    // Optional array of groups, each containing a set of related fields
  fields?: Field[];    // Optional standalone fields that are not part of any group
  mode?: "ask" | "group"; // Optional mode for the section: "ask" for questions, "group" for informational messages
}
