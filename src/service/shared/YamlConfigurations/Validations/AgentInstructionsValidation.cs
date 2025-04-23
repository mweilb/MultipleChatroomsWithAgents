namespace YamlConfigurations.Validations
{
    public class AgentInstructionsValidation : IValidationPass
    {
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {
            if (errors is not IList<ValidationError> errorList)
                throw new System.ArgumentException("errors must be a mutable collection");

            // Validate global agents.
            if (config.Agents != null)
            {
                foreach (var agentPair in config.Agents)
                {
                    var agentName = agentPair.Key;
                    var agentConfig = agentPair.Value;
                    CheckEchoAnInstructions(errorList, agentName, agentConfig);
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
                        CheckEchoAnInstructions(errorList, agentInstance.Name, agentInstance);
                    }
                }
            }
            // No return, mutate errorList in place
        }

        private static void CheckEchoAnInstructions(IList<ValidationError> errors, string agentName, YamlAgentConfig agentConfig)
        {
            bool bInstruciton = string.IsNullOrWhiteSpace(agentConfig.Instructions);
            bool bEcho = string.IsNullOrWhiteSpace(agentConfig.Echo);

            int? line = agentConfig is YamlLineInfo info ? (int?)info.StartLine : null;
            int? ch = agentConfig is YamlLineInfo info2 ? (int?)info2.StartColumn : null;

            if ((agentConfig.Instructions != null) && (agentConfig.Echo != null))
            {
                errors.Add(new ValidationError(
                    "Agent instructions and echo must not be both defined.",
                    $"[Agent:{agentName}]",
                    agentConfig,
                    ValidationErrorKeywords.Instructions
                    ));
            }
            else if ((agentConfig.Instructions != null) && bInstruciton)
            {
                errors.Add(new ValidationError(
                "Agent instructions must not be empty.",
                $"[Agent:{agentName}]",
                    agentConfig,
                    ValidationErrorKeywords.Instructions
                ));
            }
            else if ((agentConfig.Echo != null) && bEcho)
            {
                errors.Add(new ValidationError(
                "Agent Echo must not be empty.",
                $"[Agent:{agentName}]",
                agentConfig,
                ValidationErrorKeywords.Instructions
                ));
            }
        }
    }

}
