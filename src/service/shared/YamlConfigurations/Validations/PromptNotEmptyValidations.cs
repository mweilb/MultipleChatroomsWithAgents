namespace YamlConfigurations.Validations
{
    public class PromptNotEmptyValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            // Iterate over all chat rooms.
            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;

                    // Check rules (if any) in the room's strategy.
                    if (room.Strategies?.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            // Validate that the prompt for SelectAgentOrRoom is not empty.
                            if (rule.Selection != null &&
                                rule.Selection.PromptSelect != null &&
                                string.IsNullOrWhiteSpace(rule.Selection.PromptSelect.Instructions))
                            {
                                errors.Add(new ValidationError(
                                    "Prompt must not be empty.",
                                    $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].SelectAgentOrRoom.Prompt"
                                ));
                            }

                            // For Termination, if presets are available then an empty prompt is acceptable.
                            if (rule.Termination != null)
                            {
                            
                                if (rule.Termination != null &&
                                    rule.Termination.PromptTermination != null &&
                                    string.IsNullOrWhiteSpace(rule.Termination.PromptTermination.Instructions))
                                {
                                    errors.Add(new ValidationError(
                                        "Prompt must not be empty.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Prompt"
                                    ));
                                }
                            }
                        }
                    }
                }
            }
            return errors;
        }
    }
}
