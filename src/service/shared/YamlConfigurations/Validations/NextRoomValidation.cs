using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class NextRoomValidation : IValidationPass
    {
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {

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
                            foreach (var next in rule.Next)
                            {
                                // If next.Name is a room name, then check that ContextTransfer is valid.
                                if (config.Rooms.ContainsKey(next.Name))
                                {
                                    if (next.ContextTransfer == null)
                                    {
                                        errors.Add(new ValidationError(
                                            $"Next reference '{next.Name}' is a room and must have a valid ContextTransfer defined.",
                                             next 
                                        ));
                                    }
                                    else if (string.IsNullOrWhiteSpace(next.ContextTransfer.Prompt))
                                    {
                                          errors.Add(new ValidationError(
                                            $"ContextTransfer for next reference '{next.Name}' must have a non-empty prompt.",
                                            next 
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
}
