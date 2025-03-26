import { WebSocketReplyChatRoomMessage } from "shared";
import { Field } from "../../types/SectionTypes";

/**
 * Helper function to check if a field has any non-empty content.
 * Returns true if the specified field's content exists (non-empty).
 */
export const isFieldValuePresent = (
  msg: WebSocketReplyChatRoomMessage,
  fieldKey: string,
  subFieldKey: string
): boolean => !!msg.Hints?.[fieldKey]?.[subFieldKey]?.toString().trim();

/**
 * Helper function to check if any field in the provided list is populated.
 * Returns true if at least one field in the list has non-empty content.
 */
export const areAnyFieldValuesPresent = (
  msg: WebSocketReplyChatRoomMessage,
  fields: Field[]
): boolean => fields.some((field) => isFieldValuePresent(msg, field.key, field.subkey));
