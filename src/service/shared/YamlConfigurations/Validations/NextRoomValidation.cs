using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class NextRoomValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config, string? yamlText = null)
        {
            var errors = new List<ValidationError>();

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
                                            $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}].ContextTransfer"
                                        ));
                                    }
                                    else if (string.IsNullOrWhiteSpace(next.ContextTransfer.Prompt))
                                    {
                                        int? lineNum = null;
                                        int? charPos = null;
                                        if (yamlText != null)
                                        {
                                            var lines = yamlText.Split('\n');
                                            for (int i = 0; i < lines.Length; i++)
                                            {
                                                // Look for the next: block with the correct next.Name
                                                if (lines[i].Contains("next:", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    // Search for the next.Name under this block
                                                    for (int j = i + 1; j < Math.Min(i + 15, lines.Length); j++)
                                                    {
                                                        if (lines[j].Contains(next.Name, StringComparison.OrdinalIgnoreCase))
                                                        {
                                                            // Now look for prompt: under this next.Name
                                                            for (int k = j; k < Math.Min(j + 10, lines.Length); k++)
                                                            {
                                                                if (lines[k].Contains("prompt:", StringComparison.OrdinalIgnoreCase))
                                                                {
                                                                    lineNum = k + 1;
                                                                    charPos = lines[k].IndexOf("prompt:", StringComparison.OrdinalIgnoreCase) + 1;
                                                                    goto found;
                                                                }
                                                            }
                                                            // If prompt: not found, use the next.Name line
                                                            lineNum = j + 1;
                                                            charPos = lines[j].IndexOf(next.Name, StringComparison.OrdinalIgnoreCase) + 1;
                                                            goto found;
                                                        }
                                                    }
                                                }
                                            }
                                            found: ;
                                        }
                                        errors.Add(new ValidationError(
                                            $"ContextTransfer for next reference '{next.Name}' must have a non-empty prompt.",
                                            $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}].ContextTransfer.Prompt",
                                            lineNum,
                                            charPos
                                        ));
                                    }
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
