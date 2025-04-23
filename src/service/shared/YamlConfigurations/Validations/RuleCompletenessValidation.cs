using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class RuleCompletenessValidation : IValidationPass
    {
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {
            if (errors is not IList<ValidationError> errorList)
                throw new System.ArgumentException("errors must be a mutable collection");

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
                                errorList.Add(new ValidationError(
                                    "Rule must have a termination decision defined.",
                                    $"[Rule:{rule.Name}]",
                                    rule,
                                    ValidationErrorKeywords.Rule
                                ));
                            }

                            // If the selection decision is null, ensure both "current" and "next" are provided.
                            if (rule.Selection == null)
                            {
                                bool hasCurrent = rule.Current != null && rule.Current.Any();
                                bool hasNext = rule.Next != null && rule.Next.Any();

                                if (!hasCurrent || !hasNext)
                                {
errorList.Add(new ValidationError(
    "Rule must have a selection decision defined unless both current and next agents are specified.",
    $"[Rule:{rule.Name}]",
    rule,
    ValidationErrorKeywords.Rule
));
                                }
                            }
                        }
                    }
                }
            }
            // No return, mutate errorList in place
        }
    }
}
