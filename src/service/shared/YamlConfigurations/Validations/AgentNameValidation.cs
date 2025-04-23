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
        // Validate agent names in room.Agents
        if (room.Agents != null)
        {
            foreach (var agent in room.Agents)
            {
                if (!YamlInstanceOfAgentConfig.IsValidAgentName(agent.Name))
                {
                    errorList.Add(new ValidationError(
                        $"Agent name '{agent.Name}' in room '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                        $"[Room:{roomName}][Agent:{agent.Name}]",
                        agent,
                        ValidationErrorKeywords.Agent
                    ));
                }
            }
        }
        // Validate agent names in rules: current, next, and continuation-agent-name
        if (room.Strategies?.Rules != null)
        {
            foreach (var rule in room.Strategies.Rules)
            {
                // Validate current agent names
                if (rule.Current != null)
                {
                    foreach (var current in rule.Current)
                    {
                        if (!string.IsNullOrEmpty(current.Name) && !YamlInstanceOfAgentConfig.IsValidAgentName(current.Name))
                        {
                            errorList.Add(new ValidationError(
                                $"Current agent name '{current.Name}' in rule '{rule.Name}' of room '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                                $"[Room:{roomName}][Rule:{rule.Name}][Current:{current.Name}]",
                                current,
                                ValidationErrorKeywords.Current
                            ));
                        }
                    }
                }
                // Validate next agent names
                if (rule.Next != null)
                {
                    foreach (var next in rule.Next)
                    {
                        if (!string.IsNullOrEmpty(next.Name) && !YamlInstanceOfAgentConfig.IsValidAgentName(next.Name))
                        {
                            errorList.Add(new ValidationError(
                                $"Next agent name '{next.Name}' in rule '{rule.Name}' of room '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                                $"[Room:{roomName}][Rule:{rule.Name}][Next:{next.Name}]",
                                next,
                                ValidationErrorKeywords.Next
                            ));
                        }
                    }
                }
                // Validate continuation-agent-name in termination
                if (rule.Termination != null && !string.IsNullOrEmpty(rule.Termination.ContinuationAgentName))
                {
                    var contName = rule.Termination.ContinuationAgentName;
                    if (!YamlInstanceOfAgentConfig.IsValidAgentName(contName))
                    {
                        errorList.Add(new ValidationError(
                            $"Continuation agent name '{contName}' in rule '{rule.Name}' of room '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                            $"[Room:{roomName}][Rule:{rule.Name}][ContinuationAgentName:{contName}]",
                            rule.Termination,
                            ValidationErrorKeywords.Termination
                        ));
                    }
                }
            }
        }
    }
}
            // No return, mutate errorList in place
        }
    }
}
