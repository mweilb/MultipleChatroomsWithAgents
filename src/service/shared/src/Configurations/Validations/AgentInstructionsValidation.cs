 

namespace MultiAgents.Configurations.Validations
{
    public class AgentInstructionsValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            // Validate global agents.
            if (config.Agents != null)
            {
                foreach (var agentPair in config.Agents)
                {
                    var agentName = agentPair.Key;
                    var agentConfig = agentPair.Value;
                    if (string.IsNullOrWhiteSpace(agentConfig.Instructions))
                    {
                        errors.Add(new ValidationError(
                            "Agent instructions must not be empty.",
                            $"Agents[{agentName}].Instructions"
                        ));
                    }
                }
            }

            // Validate agents inside each room.
            if (config.Rooms != null)
            {
                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;
                    foreach (var agentInstance in room.Agents)
                    {
                        if (string.IsNullOrWhiteSpace(agentInstance.Instructions))
                        {
                            errors.Add(new ValidationError(
                                "Agent instructions must not be empty.",
                                $"Rooms[{roomName}].Agents[{agentInstance.Name}].Instructions"
                            ));
                        }
                    }
                }
            }
            return errors;
        }
    }

}
