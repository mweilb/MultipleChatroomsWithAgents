﻿using System;
using System.Collections.Generic;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class AgentReferenceValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config, string? yamlText = null)
        {
            var errors = new List<ValidationError>();
            var errorKeys = new HashSet<string>();

            bool IsTrue(string? value)
            {
                return value != null &&
                       (value.Contains("yes", StringComparison.OrdinalIgnoreCase) ||
                        value.Contains("true", StringComparison.OrdinalIgnoreCase));
            }

            void AddError(ValidationError err)
            {
                var key = $"{err.Message}|{err.LineNumber}|{err.CharPosition}";
                if (errorKeys.Add(key))
                    errors.Add(err);
            }

            (int? line, int? ch) FindLineAndCharInYaml(string? yaml, string search)
            {
                if (yaml == null) return (null, null);
                var lines = yaml.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var idx = lines[i].IndexOf(search, StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                        return (i + 1, idx + 1);
                }
                return (null, null);
            }

            bool IsValidReference(string name, HashSet<string> validAgentNames, HashSet<string> validRoomNames, HashSet<string?>? validTerminationNames)
            {
                return validAgentNames.Contains(name)
                    || validRoomNames.Contains(name)
                    || (validTerminationNames != null && validTerminationNames.Contains(name))
                    || name.Equals("start", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("any", StringComparison.OrdinalIgnoreCase);
            }


            bool AllNextRefsHaveContextTransfer(YamlStratergyRules rule)
            {
                var nextRefs = new List<dynamic>();

                foreach (var next in rule.Next)
                {
                    nextRefs.Add(next);
                }
                return nextRefs.Count > 0 && nextRefs.All(n => n.ContextTransfer != null && !string.IsNullOrWhiteSpace(n.ContextTransfer.Prompt));
            }

            void AddErrorWithYamlLocation(string message, string path, string? yaml, string search, Action<ValidationError> add)
            {
                var (line, ch) = FindLineAndCharInYaml(yaml, search);
                add(new ValidationError(message, path, line, ch));
            }

            if (config.Rooms != null)
            {
                var validRoomNames = config.Rooms.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(config.StartRoom) && !validRoomNames.Contains(config.StartRoom))
                {
                    AddError(new ValidationError(
                        $"StartRoom is not a valid room: '{config.StartRoom}'",
                        $"[{config.Name}].StartRoom"
                    ));
                }

                foreach (var roomPair in config.Rooms)
                {
                    var roomName = roomPair.Key;
                    var room = roomPair.Value;
                    var validAgentNames = room.Agents.Select(a => a.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var validTerminationNames = room.Strategies?.Rules
                        .Where(rule => rule.Termination != null)
                        .Select(rule => rule.Termination?.ContinuationAgentName)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (room.Strategies?.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            foreach (var current in rule.Current)
                            {
                                if (!IsValidReference(current.Name, validAgentNames, validRoomNames, validTerminationNames))
                                {
                                    AddErrorWithYamlLocation(
                                        $"Current reference '{current.Name}' is not a valid agent or room in room '{roomName}'.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Current[{current.Name}]",
                                        yamlText,
                                        current.Name,
                                        AddError
                                    );
                                }
                            }

                            foreach (var next in rule.Next)
                            {
                                if (!IsValidReference(next.Name, validAgentNames, validRoomNames, validTerminationNames))
                                {
                                    AddErrorWithYamlLocation(
                                        $"Next reference '{next.Name}' is not a valid agent or room in room '{roomName}'.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}]",
                                        yamlText,
                                        next.Name,
                                        AddError
                                    );
                                }

                                if (validTerminationNames != null && validTerminationNames.Contains(next.Name))
                                {
                                    AddErrorWithYamlLocation(
                                        $"Next reference '{next.Name}' is Termination Name and can not be used in next statement.",
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}]",
                                        yamlText,
                                        next.Name,
                                        AddError
                                    );
                                }
                            }
                        }

                        foreach (var rule in room.Strategies.Rules)
                        {
                            var termination = rule.Termination;
                            if (termination != null && !string.IsNullOrEmpty(termination.ContinuationAgentName))
                            {
                                var referencedAgentNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                foreach (var r in room.Strategies.Rules)
                                {
                                    foreach (var curr in r.Current)
                                        referencedAgentNames.Add(curr.Name);
                                    if (r.Current.Any(c => c.Name.Equals("any", StringComparison.OrdinalIgnoreCase)))
                                        foreach (var agent in validAgentNames)
                                            referencedAgentNames.Add(agent);
                                }

                                if (!referencedAgentNames.Contains(termination.ContinuationAgentName))
                                {
                                    // Only skip error if all next refs have context-transfer
                                    if (!AllNextRefsHaveContextTransfer(rule))
                                    {
                                        //if constant, could be automatically 
                                        if ((termination.ConstantTermination == null) || (IsTrue(termination.ConstantTermination.Value)))
                                        {
                                            AddErrorWithYamlLocation(
                                                $"ContinuationAgentName '{termination.ContinuationAgentName}' is not reference in any rule.",
                                                $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.ContinuationAgentName",
                                                yamlText,
                                                termination.ContinuationAgentName,
                                                AddError
                                            );
                                        }
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
