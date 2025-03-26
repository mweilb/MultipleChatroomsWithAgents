namespace MultiAgents.Configurations.Validations
{
    public class RuleCompletenessValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;

                    if (room.Strategies?.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            // Always require a termination decision.
                            if (rule.Termination == null)
                            {
                                errors.Add(new ValidationError(
                                    "Rule must have a termination decision defined.",
                                    $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination"
                                ));
                            }

                            // If the selection decision is null, ensure both "current" and "next" are provided.
                            if (rule.SelectAgentOrRoom == null)
                            {
                                bool hasCurrent = rule.Current != null && rule.Current.Any();
                                bool hasNext = rule.Next != null && rule.Next.Any();

                                if (!hasCurrent || !hasNext)
                                {
                                    errors.Add(new ValidationError(
                                        "Rule must have a selection decision defined unless both current and next agents are specified.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].SelectAgentOrRoom"
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
