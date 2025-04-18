using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class AgentNameValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config, string? yamlText = null)
        {
            var errors = new List<ValidationError>();

            (int? line, int? ch) FindLineAndCharInYaml(string? yaml, string search)
            {
                if (yaml == null) return (null, null);
                var lines = yaml.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var idx = lines[i].IndexOf(search, System.StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                        return (i + 1, idx + 1);
                }
                return (null, null);
            }

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
                                var (line, ch) = FindLineAndCharInYaml(yamlText, agent.Name);
                                errors.Add(new ValidationError(
                                    $"Agent name '{agent.Name}' in room '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                                    $"Rooms[{roomName}].Agents[{agent.Name}]",
                                    line,
                                    ch
                                ));
                            }
                        }
                    }
                }
            }

            return errors;
        }
    }
}
