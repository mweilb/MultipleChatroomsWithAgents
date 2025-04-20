using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class AgentNameValidation : IValidationPass
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
                    if (room.Agents != null)
                    {
                        foreach (var agent in room.Agents)
                        {
                            if (!YamlInstanceOfAgentConfig.IsValidAgentName(agent.Name))
                            {
errorList.Add(new ValidationError(
    $"Agent name '{agent.Name}' in room '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
    agent
));
                            }
                        }
                    }
                }
            }
            // No return, mutate errorList in place
        }
    }
}
