namespace YamlConfigurations.Validations
{
    public class PromptNotEmptyValidation : IValidationPass
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
                                    $"[Room:{roomName}][Rule:{rule.Name}][Selection]",
                                    rule.Selection,
                                    ValidationErrorKeywords.Selection
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
                                        $"[Room:{roomName}][Rule:{rule.Name}][Termination]",
                                        rule.Termination,
                                        ValidationErrorKeywords.Termination
                                    ));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
