namespace YamlConfigurations.Validations
{
    public class ModerationPromptNotEmptyValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config, string? yamlText = null)
        {
            var errors = new List<ValidationError>();

            // Iterate over all rooms.
            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;

                    // If Moderation exists, ensure its prompt is not empty.
                    if (room.Moderation != null && string.IsNullOrWhiteSpace(room.Moderation.Prompt))
                    {
                        errors.Add(new ValidationError(
                            "Moderation prompt must not be empty.",
                            $"Rooms[{roomName}].Moderation.Prompt"
                        ));
                    }
                }
            }

            return errors;
        }
    }
}
