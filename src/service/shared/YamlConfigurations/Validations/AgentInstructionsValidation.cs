namespace YamlConfigurations.Validations
{
    public class AgentInstructionsValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config, string? yamlText = null)
        {
            var errors = new List<ValidationError>();

            // Validate global agents.
            if (config.Agents != null)
            {
                foreach (var agentPair in config.Agents)
                {
                    var agentName = agentPair.Key;
                    var agentConfig = agentPair.Value;
                    CheckEchoAnInstructions(errors, agentName, agentConfig, yamlText);
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
                        CheckEchoAnInstructions(errors, agentInstance.Name, agentInstance, yamlText);
                    }
                }
            }
            return errors;
        }

        private static void CheckEchoAnInstructions(List<ValidationError> errors, string agentName, YamlAgentConfig agentConfig, string? yamlText)
        {
            bool bInstruciton = string.IsNullOrWhiteSpace(agentConfig.Instructions);
            bool bEcho = string.IsNullOrWhiteSpace(agentConfig.Echo);

            int? instrLine = null, instrChar = null, echoLine = null, echoChar = null;
            if (yamlText != null)
            {
                var lines = yamlText.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var instrIdx = lines[i].IndexOf($"instructions:", StringComparison.OrdinalIgnoreCase);
                    if (instrIdx >= 0 && lines[i].IndexOf(agentName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        instrLine = i + 1;
                        instrChar = instrIdx + 1;
                    }
                    var echoIdx = lines[i].IndexOf($"echo:", StringComparison.OrdinalIgnoreCase);
                    if (echoIdx >= 0 && lines[i].IndexOf(agentName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        echoLine = i + 1;
                        echoChar = echoIdx + 1;
                    }
                }
            }

            if ((agentConfig.Instructions != null) && (agentConfig.Echo != null))
            {
                errors.Add(new ValidationError(
                       "Agent instructions and echo must not be both defined.",
                       $"Agents[{agentName}].Instructions",
                       instrLine,
                       instrChar
                    ));
            }
            else if ((agentConfig.Instructions != null) && bInstruciton)
            {
                errors.Add(new ValidationError(
                   "Agent instructions must not be empty.",
                   $"Agents[{agentName}].Instructions",
                   instrLine,
                   instrChar
               ));
            }
            else if ((agentConfig.Echo != null) && bEcho)
            {
                errors.Add(new ValidationError(
                   "Agent Echo must not be empty.",
                   $"Agents[{agentName}].Echo",
                   echoLine,
                   echoChar
               ));
            }
        }
    }

}
