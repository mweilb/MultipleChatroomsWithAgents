using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiAgents.Configurations.Validations
{
    public class MessagesPresetFiltersValidation : IValidationPass
    {
        // Allowed values for messages-preset-filters.
        private readonly HashSet<string> validFilters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Last message",
            "Remove content"
        };

        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            // Iterate over each room to check decision configurations.
            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;

                    if (room.Strategies != null)
                    {
                        // Validate global decision configs.

                        ValidateDecisionConfig(room.Strategies.GlobalSelectAgentOrRoom,
                            $"Rooms[{roomName}].Strategies.GlobalSelectAgentOrRoom", errors);
                        ValidateDecisionConfig(room.Strategies.GlobalTermination,
                            $"Rooms[{roomName}].Strategies.GlobalTermination", errors);

                        if (room.Strategies.Rules != null)
                        {
                            foreach (var rule in room.Strategies.Rules)
                            {
                                // Validate each rule's decision configs.
                                ValidateDecisionConfig(rule.SelectAgentOrRoom,
                                    $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].SelectAgentOrRoom", errors);
                                ValidateDecisionConfig(rule.Termination,
                                    $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination", errors);

                                if (rule.Next != null)
                                {
                                    foreach (var next in rule.Next)
                                    {
                                        ValidateDecisionConfig(next.ContextTransfer,
                                            $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}].ContextTransfer", errors);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return errors;
        }

        private void ValidateDecisionConfig(YamlDecisionConfig? decisionConfig, string location, List<ValidationError> errors)
        {
            if (decisionConfig == null)
            {
                return;
            }

            if (decisionConfig.MessagePresetFilters != null)
            {
                foreach (var filter in decisionConfig.MessagePresetFilters)
                {
                    if (!validFilters.Contains(filter))
                    {
                        errors.Add(new ValidationError(
                            $"Invalid messages-preset-filter '{filter}'. Valid options are: {string.Join(", ", validFilters)}.",
                            $"{location}.MessagePresetFilters"
                        ));
                    }
                }
            }
        }
    }
}
