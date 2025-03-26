import React from 'react';
import { WebSocketReplyChatRoomMessage } from 'shared';
import MessageSection from './MessageSection';
import { messageSectionDefinitions } from '../../configs/SectionDefinitions';
import { Section, Field } from '../../types/SectionTypes';
import { areAnyFieldValuesPresent, isFieldValuePresent } from './MessageUtils';

interface MessageSectionsProps {
    msg: WebSocketReplyChatRoomMessage;         /** The message object containing the content and metadata for rendering. This will contain sections and fields that need to be dynamically rendered. */
    collapsedSections: Set<string>;             /** A set of section titles that are currently collapsed. Controls the visibility of sections based on user interactions. */
    collapsedFields: Set<string>;               /** A set of field labels that are currently collapsed. Controls the visibility of individual fields within sections. */
}
  
/**
 * Determines whether a given section should be rendered.
 * A section is rendered if it has any groups or standalone fields with content.
 *
 * @param section The section to evaluate.
 * @param msg The message object containing the data for the section.
 * @returns `true` if the section has content to display, otherwise `false`.
 */
const shouldRenderSection = (section: Section, msg: WebSocketReplyChatRoomMessage): boolean => {
  // Render the section only if it matches the SubAction and contains populated fields or groups
  if (section.mode === 'ask' && msg.SubAction !== 'ask') return false;
  if (section.mode === 'group' && msg.SubAction === 'ask') return false;

  // Check for populated groups or fields
  const groupsPopulated =
    section.groups?.some((group) => areAnyFieldValuesPresent(msg, group.fields)) || false;
  const standalonePopulated =
    section.fields?.some((field) => isFieldValuePresent(msg, field.key, field.subkey)) || false;

  return groupsPopulated || standalonePopulated;
};

/**
 * Renders the fields within a group.
 *
 * @param group The group containing fields to be rendered.
 * @param msg The message object with content for the fields.
 * @param collapsedFields The set of collapsed field labels.
 * @param transactionId The transaction ID for uniquely identifying the field.
 * @returns An array of MessageSection components for each valid field.
 */
const renderGroupFields = (
    group: { name: string; fields: Field[] },
    msg: WebSocketReplyChatRoomMessage,
    collapsedFields: Set<string>,
    transactionId: string
  ) => {
    return group.fields
      .map((field) => {
        if (!isFieldValuePresent(msg, field.key, field.subkey) || collapsedFields.has(field.label)) return null;
        return (
          <MessageSection
            styleKey={field.label.toLowerCase().replace(/\s/g, '-') + '-label'}
            key={field.label}
            title={field.label}
            useMarkdown={true}
            markdownContent={msg.Hints?.[field.key]?.[field.subkey]?.toString()}
            localStorageKey={`${field.key}-${transactionId}`}
          />
        );
      })
      .filter((element) => element !== null);
  };
  
  /**
   * Renders a group as a single MessageSection that includes all of its fields.
   *
   * @param group The group object containing a name and fields.
   * @param msg The message object containing the data for the fields.
   * @param collapsedFields The set of collapsed field labels.
   * @param transactionId The unique transaction ID.
   * @returns A MessageSection wrapping the group's fields, or null if no fields should be rendered.
   */
  const renderGroup = (
    group: { name: string; key:string, fields: Field[] },
    msg: WebSocketReplyChatRoomMessage,
    collapsedFields: Set<string>,
    transactionId: string
  ) => {
    // Check if any fields in the group have content.
    if (!areAnyFieldValuesPresent(msg, group.fields)) return null;
  
    // Render the fields within the group.
    const fieldElements = renderGroupFields(group, msg, collapsedFields, transactionId);
  
    // If no valid fields are rendered, skip the group.
    if (fieldElements.length === 0) return null;
  
    // Wrap all field elements in a single group-level MessageSection.
    return (
      <MessageSection
        styleKey={group.key + '-group'}
        title={group.name}
        localStorageKey={`${group.name}-${transactionId}`}
      >
        {fieldElements}
      </MessageSection>
    );
  };
  
/**
 * Renders a standalone field (not part of a group). It checks if the field has content and if it is collapsed
 * before rendering it.
 *
 * @param field The field to be rendered.
 * @param msg The message object containing the field data.
 * @param collapsedFields The set of collapsed field labels.
 * @param transactionId The unique transaction ID to identify this field.
 * @returns A `MessageSection` component if the field has content and is not collapsed; otherwise, `null`.
 */
const renderStandaloneField = (field: Field, msg: WebSocketReplyChatRoomMessage, collapsedFields: Set<string>, transactionId: string) => {
  // Skip rendering if the field has no content or is collapsed
  if (!isFieldValuePresent(msg, field.key, field.subkey) || collapsedFields.has(field.label)) return null;

  // Return a rendered MessageSection for the field
  return (
    <MessageSection
      styleKey={field.label.toLowerCase().replace(/\s/g, '-') + '-label'}
      key={field.label}
      title={field.label}
      useMarkdown={true}
      markdownContent={msg.Hints?.[field.key]?.[field.subkey]?.toString()}
      localStorageKey={`${field.key}-${transactionId}`}
    />
  );
};

/**
 * Renders a section, which includes its title, subtitle, and the fields and groups within it.
 * The section can be collapsed or expanded based on the `collapsedSections` state.
 *
 * @param section The section to be rendered.
 * @param msg The message object containing data for this section.
 * @param collapsedSections The set of collapsed section titles.
 * @param collapsedFields The set of collapsed field labels.
 * @param transactionId The unique transaction ID for storing local state.
 * @returns A `div` element containing the section, with its groups and fields.
 */
const renderSection = (section: Section, msg: WebSocketReplyChatRoomMessage, collapsedSections: Set<string>, collapsedFields: Set<string>, transactionId: string) => {
  // Determine whether the section should be collapsed
  const isHidden = collapsedSections.has(section.key);
 
  
  // Return the section with its content, applying appropriate styles for collapsed/expanded states
  return (
    <div key={section.title} className={`rationale-section ${section.key?.toLowerCase()}`} style={{ display: isHidden ? 'none' : 'block' }}>
      <MessageSection
        styleKey={section.title.toLowerCase().replace(/\s/g, '-') + '-section'}
        title={section.title}
        subtitle={section.key ? msg.Hints[section.key]?.toString() : ''}
        localStorageKey={`${section.title.toLowerCase().replace(/\s/g, '-')}-${transactionId}`}
      >
        {/* Render the groups within the section */}
        {section.groups?.map((group, idx) => {
          const isGroupHidden = collapsedSections.has(group.key);
          if (isGroupHidden) return null;
          return renderGroup(group, msg, collapsedFields, transactionId);
        })}

        {/* Render standalone fields within the section */}
        {section.fields?.map((field, idx) => {
          return renderStandaloneField(field, msg, collapsedFields, transactionId);
        })}
      </MessageSection>
    </div>
  );
};

/**
 * The main component responsible for rendering all sections of a message. 
 * It filters sections based on their content and renders them conditionally.
 *
 * @param msg The message containing the sections to render.
 * @param collapsedSections The set of collapsed section titles.
 * @param collapsedFields The set of collapsed field labels.
 * @returns A `div` containing all the sections of the message.
 */
const MessageSections: React.FC<MessageSectionsProps> = ({ msg, collapsedSections, collapsedFields }) => {
  // Filter out sections that should not be rendered based on content and mode
  const filteredSections = messageSectionDefinitions.filter((section) => shouldRenderSection(section, msg));

  // If no sections are available to render, return null
  if (filteredSections.length === 0) return null;

  // Render the filtered sections
  return (
    <div className="rationale">
      {filteredSections.map((section) => {
        return renderSection(section, msg, collapsedSections, collapsedFields, msg.TransactionId);
      })}
    </div>
  );
};

export default MessageSections;
