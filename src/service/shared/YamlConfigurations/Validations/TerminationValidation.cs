﻿﻿﻿﻿﻿﻿ 

namespace YamlConfigurations.Validations
{
    public class TerminationValidation : IValidationPass
    {
   
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors )
        {
            if (config.Rooms == null)
            {
                return;
            }

            foreach (var roomPair in config.Rooms)
            {
                var roomName = roomPair.Key;
                var room = roomPair.Value;

                // Build a set of valid agent names for the room.
                var validAgentNames = room.Agents != null
                    ? new HashSet<string>(room.Agents.Select(a => a.Name), StringComparer.OrdinalIgnoreCase)
                    : [];

                if (room.Strategies != null)
                {
                    // If a global termination is defined, validate it and compare with child rules.
                    if (room.Strategies.GlobalTermination is YamlTerminationDecisionConfig globalTerm)
                    {
                        ValidateTerminations(globalTerm, validAgentNames, errors, $"Room:{roomName}:GlobalTermination");

                        foreach (var rule in room.Strategies.Rules)
                        {
                            if (rule.Termination is YamlTerminationDecisionConfig ruleTerm)
                            {
                                if (ruleTerm != globalTerm)
                                {
                                    ValidateTerminations(ruleTerm, validAgentNames, errors, $"Room:{roomName}:Rule:{rule.Name}");
                                }
                            }
                        }

                    }
                    else if (room.Strategies.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            if (rule.Termination is YamlTerminationDecisionConfig ruleTerm)
                            {
                                ValidateTerminations(ruleTerm, validAgentNames, errors, $"Room:{roomName}:Rule:{rule.Name}");
                            }
                        }
                    }


                }
            }
        }

        private void ValidateTerminations(
            YamlTerminationDecisionConfig termConfig,
            HashSet<string> validAgentNames,
            IList<ValidationError> errors,
            string parentLocation)
        {
            // Track which members are not null
            var nonNullMembers = new List<string>();

            if (termConfig.RegexTermination != null)
                nonNullMembers.Add("regex-termination");

            if (termConfig.ConstantTermination != null)
                nonNullMembers.Add("constant-termination");

            if (termConfig.PromptTermination != null)
                nonNullMembers.Add("prompt-termination");

            // If more than one is set, it's an error
            if (nonNullMembers.Count > 1)
            {
                errors.Add(new ValidationError(
                    $"Only one termination type may be specified, but found multiple: {string.Join(", ", nonNullMembers)}.",
                    $"{parentLocation}[Termination:{termConfig.ContinuationAgentName}]",
                    termConfig,
                    ValidationErrorKeywords.Termination
                ));
            }

            // RegexTermination validation
            if (termConfig.RegexTermination != null)
            {
                var regexTerm = termConfig.RegexTermination;
                if (regexTerm.Patterns == null || regexTerm.Patterns.Count == 0)
                {
                    errors.Add(new ValidationError(
                        "RegexTermination: 'expressions' must be specified and non-empty.",
                        parentLocation,
                        regexTerm,
                        ValidationErrorKeywords.RegexTermination
                    ));
                }
                else
                {
                    foreach (var pattern in regexTerm.Patterns)
                    {
                        try
                        {
                            _ = new System.Text.RegularExpressions.Regex(pattern);
                        }
                        catch
                        {
                            errors.Add(new ValidationError(
                                $"RegexTermination: Invalid regex pattern: '{pattern}'",
                                parentLocation,
                                regexTerm,
                                ValidationErrorKeywords.RegexTermination
                            ));
                        }
                    }
                }
            }

            // ConstantTermination validation
            if (termConfig.ConstantTermination != null)
            {
                var constTerm = termConfig.ConstantTermination;
                if (constTerm.Agents == null || constTerm.Agents.Count == 0)
                {
                    errors.Add(new ValidationError(
                        "ConstantTermination: 'agents' must be specified and non-empty.",
                        parentLocation,
                        constTerm,
                        ValidationErrorKeywords.ConstantTermination
                    ));
                }
                else
                {
                    foreach (var agent in constTerm.Agents)
                    {
                        if (!validAgentNames.Contains(agent))
                        {
                            errors.Add(new ValidationError(
                                $"ConstantTermination: Agent '{agent}' does not exist in this room.",
                                parentLocation,
                                constTerm,
                                ValidationErrorKeywords.ConstantTermination
                            ));
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(constTerm.Value))
                {
                    errors.Add(new ValidationError(
                        "ConstantTermination: 'value' must be specified and non-empty.",
                        parentLocation,
                        constTerm,
                        ValidationErrorKeywords.ConstantTermination
                    ));
                }
            }

            // PromptTermination validation
            if (termConfig.PromptTermination != null)
            {
                var promptTerm = termConfig.PromptTermination;
                if (promptTerm.Agents == null || promptTerm.Agents.Count == 0)
                {
                    errors.Add(new ValidationError(
                        "PromptTermination: 'agents' must be specified and non-empty.",
                        parentLocation,
                        promptTerm,
                        ValidationErrorKeywords.PromptSelect
                    ));
                }
                else
                {
                    foreach (var agent in promptTerm.Agents)
                    {
                        if (!validAgentNames.Contains(agent))
                        {
                            errors.Add(new ValidationError(
                                $"PromptTermination: Agent '{agent}' does not exist in this room.",
                                parentLocation,
                                promptTerm,
                                ValidationErrorKeywords.PromptTermination
                            ));
                        }
                    }
                }
                if (string.IsNullOrWhiteSpace(promptTerm.Instructions))
                {
                    errors.Add(new ValidationError(
                        "PromptTermination: 'instructions' must be specified and non-empty.",
                        parentLocation,
                        promptTerm,
                        ValidationErrorKeywords.PromptTermination
                    ));
                }
            }
        }

    }
}
