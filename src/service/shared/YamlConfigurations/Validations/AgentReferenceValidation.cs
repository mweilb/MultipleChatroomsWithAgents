using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class AgentReferenceValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            if (config.Rooms != null)
            {
                // Extract valid room names from the configuration.
                var validRoomNames = config.Rooms.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);


                if (string.IsNullOrEmpty(config.StartRoom) == false){
                    if (validRoomNames.Contains(config.StartRoom) == false)
                    {
                        errors.Add(new ValidationError(
                            $"StartRoom is not a valid room: '{config.StartRoom}'",
                            $"[{config.Name}].StartRoom"
                        ));
                    }
                }


                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;

                    // Collect valid agent names defined within the current room.
                    var validAgentNames = room.Agents.Select(a => a.Name)
                                                     .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    var validTerminationNames = room.Strategies?.Rules
                       .Where(rule => rule.Termination != null)
                       .Select(rule => rule.Termination?.ContinuationAgentName)
                       .ToHashSet(StringComparer.OrdinalIgnoreCase);


                    // A helper function that validates the reference name.
                    bool IsValidReference(string name) =>
                        validAgentNames.Contains(name) ||
                        validRoomNames.Contains(name) ||
                        validTerminationNames != null && validTerminationNames.Contains(name) ||
                        name.Equals("start", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("any", StringComparison.OrdinalIgnoreCase);

                    // Validate "current" references in each rule.
                    if (room.Strategies?.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            foreach (var current in rule.Current)
                            {
                                if (!IsValidReference(current.Name))
                                {
                                    errors.Add(new ValidationError(
                                        $"Current reference '{current.Name}' is not a valid agent or room in room '{roomName}'.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Current[{current.Name}]"
                                    ));
                                }
                            }

                            // Validate "next" references in each rule.
                            foreach (var next in rule.Next)
                            {
                                if (!IsValidReference(next.Name))
                                {
                                    errors.Add(new ValidationError(
                                        $"Next reference '{next.Name}' is not a valid agent or room in room '{roomName}'.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}]"
                                    ));
                                }

                                if (validTerminationNames != null && validTerminationNames.Contains(next.Name)){
                                    errors.Add(new ValidationError(
                                      $"Next reference '{next.Name}' is Termination Name and can not be used in next statement.",
                                       $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}]"
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
