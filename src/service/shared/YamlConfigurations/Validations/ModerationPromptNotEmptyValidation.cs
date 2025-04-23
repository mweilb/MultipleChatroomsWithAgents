using System.Collections.Generic;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class ModerationPromptNotEmptyValidation : IValidationPass
    {
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {
            // Iterate over all chat rooms.
            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;

                    // Check for Moderation prompt in the room's configuration.
                    if (room.Moderation != null)
                    {
                        if (string.IsNullOrWhiteSpace(room.Moderation.Prompt))
                        {
                            errors.Add(new ValidationError(
                                "Moderation prompt must not be empty.",
                                $"[Room:{roomName}][Moderation]",
                                room.Moderation,
                                ValidationErrorKeywords.Prompt
                            ));
                        }
                    }
                }
            }
        }
    }
}
