namespace MultiAgents.Configurations.Validations
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
                            if (rule.SelectAgentOrRoom != null && string.IsNullOrWhiteSpace(rule.SelectAgentOrRoom.Prompt))
                            {
                                errors.Add(new ValidationError(
                                    "Prompt must not be empty.",
                                    $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].SelectAgentOrRoom.Prompt"
                                ));
                            }

                            // For Termination, if presets are available then an empty prompt is acceptable.
                            if (rule.Termination != null)
                            {
                                if (rule.Termination is YamlTerminationDecisionConfig termConfig)
                                {
                                    // If there are no presets, then the termination prompt must not be empty.
                                    if ((termConfig.Presets == null || termConfig.Presets.Count == 0) &&
                                        string.IsNullOrWhiteSpace(termConfig.Prompt))
                                    {
                                        errors.Add(new ValidationError(
                                            "Termination prompt must not be empty when no presets are defined.",
                                            $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Prompt"
                                        ));
                                    }
                                }
                                else
                                {
                                    // If termination is not a YamlTerminationDecisionConfig, always check the prompt.
                                    if (string.IsNullOrWhiteSpace(rule.Termination.Prompt))
                                    {
                                        errors.Add(new ValidationError(
                                            "Termination prompt must not be empty.",
                                            $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Prompt"
                                        ));
                                    }
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
