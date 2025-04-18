using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class RoomNameValidation : IValidationPass
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
                    if (!YamlInstanceOfAgentConfig.IsValidRoomName(roomName))
                    {
                        var (line, ch) = FindLineAndCharInYaml(yamlText, roomName);
                        errors.Add(new ValidationError(
                            $"Room name '{roomName}' is invalid. Names must not contain spaces or any of: < | \\ / >",
                            $"Rooms[{roomName}]",
                            line,
                            ch
                        ));
                    }
                }
            }

            return errors;
        }
    }
}
