using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class NameCollisionValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            // Dictionary to track names and their assigned category (e.g., "room", "agent", or "termination").
            var usedNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (config.Rooms != null)
            {
                // Iterate over each room in the configuration.
                foreach (var roomPair in config.Rooms)
                {
                    var roomKey = roomPair.Key;
                    var room = roomPair.Value;

                    // Validate room name.
                    if (!string.IsNullOrEmpty(room.Name))
                    {
                        if (usedNames.TryGetValue(room.Name, out string? existingCategory))
                        {
                            if (!existingCategory.Equals("room", StringComparison.OrdinalIgnoreCase))
                            {
                                errors.Add(new ValidationError(
                                    $"Name collision: '{room.Name}' is used for both a room and a {existingCategory}.",
                                    $"Rooms[{roomKey}].Name"
                                ));
                            }
                        }
                        else
                        {
                            usedNames[room.Name] = "room";
                        }
                    }

                    // Validate agent names defined within the current room.
                    if (room.Agents != null)
                    {
                        foreach (var agent in room.Agents)
                        {
                            if (!string.IsNullOrEmpty(agent.Name))
                            {
                                if (usedNames.TryGetValue(agent.Name, out string? existingCategory))
                                {
                                    if (!existingCategory.Equals("agent", StringComparison.OrdinalIgnoreCase))
                                    {
                                        errors.Add(new ValidationError(
                                            $"Name collision: '{agent.Name}' is used for both an agent and a {existingCategory}.",
                                            $"Rooms[{roomKey}].Agents[{agent.Name}]"
                                        ));
                                    }
                                }
                                else
                                {
                                    usedNames[agent.Name] = "agent";
                                }
                            }
                        }
                    }

                    // Validate termination names from the strategy rules in the current room.
                    if (room.Strategies?.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            if (rule.Termination != null && !string.IsNullOrEmpty(rule.Termination.ContinuationAgentName))
                            {
                                var terminationName = rule.Termination.ContinuationAgentName;
                                if (usedNames.TryGetValue(terminationName, out string? existingCategory))
                                {
                                    if (!existingCategory.Equals("termination", StringComparison.OrdinalIgnoreCase))
                                    {
                                        errors.Add(new ValidationError(
                                            $"Name collision: '{terminationName}' is used for both a termination and a {existingCategory}.",
                                            $"Rooms[{roomKey}].Strategies.Rule[{rule.Name}].Termination.Name"
                                        ));
                                    }
                                }
                                else
                                {
                                    usedNames[terminationName] = "termination";
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
