namespace YamlConfigurations.Validations
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
                    CheckEchoAnInstructions(errors, agentName, agentConfig);

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
                        CheckEchoAnInstructions(errors, agentInstance.Name, agentInstance);
                    }
                }
            }
            return errors;
        }

        private static void CheckEchoAnInstructions(List<ValidationError> errors, string agentName, YamlAgentConfig agentConfig)
        {
            bool bInstruciton = string.IsNullOrWhiteSpace(agentConfig.Instructions);
            bool bEcho = string.IsNullOrWhiteSpace(agentConfig.Echo);

            if ((agentConfig.Instructions != null) && (agentConfig.Echo != null))
            {
                errors.Add(new ValidationError(
                       "Agent instructions and echo must not be both defined.",
                       $"Agents[{agentName}].Instructions"
                    ));
            }
            else if ((agentConfig.Instructions != null) && bInstruciton)
            {
                errors.Add(new ValidationError(
                   "Agent instructions must not be empty.",
                   $"Agents[{agentName}].Instructions"
               ));
            }
            else if ((agentConfig.Echo != null) && bEcho)
            {
                errors.Add(new ValidationError(
                   "Agent Echo must not be empty.",
                   $"Agents[{agentName}].Echo"
               ));
            }
        }
    }

}
