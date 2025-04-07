using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class SelectionValidation : IValidationPass
    {
        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();

            if (config.Rooms == null)
            {
                return errors;
            }

            foreach (var roomPair in config.Rooms)
            {
                var roomName = roomPair.Key;
                var room = roomPair.Value;

                // Build a set of valid agent names for the room.
                var validAgentNames = room.Agents != null
                    ? new HashSet<string>(room.Agents.Select(a => a.Name), StringComparer.OrdinalIgnoreCase)
                    : [];

                if (room.Strategies != null)
                {
                    // If a global termination is defined, validate it and compare with child rules.
                    if (room.Strategies.GlobalSelection is YamlSelectionConfig globalTerm)
                    {
                        string globalLocation = $"Rooms[{roomName}].Strategies.GlobalTermination.Termination";

                        ValidateSelections(globalTerm, globalLocation, validAgentNames, errors);


                        foreach (var rule in room.Strategies.Rules)
                        {
                            string ruleLocation = $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Termination";
                            if (rule.Selection is YamlSelectionConfig ruleTerm)
                            {
                                if (ruleTerm != globalTerm)
                                {
                                    ValidateSelections(ruleTerm, ruleLocation, validAgentNames, errors);
                                }
                            }
                        }

                    }
                    else if (room.Strategies.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            string ruleLocation = $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Termination";
                            if (rule.Selection is YamlSelectionConfig ruleTerm)
                            {
                                ValidateSelections(ruleTerm, ruleLocation, validAgentNames, errors);
                            }
                        }
                    }
                     
   
                }
            }

            return errors;
        }



        private void ValidateSelections(YamlSelectionConfig selection,string location, HashSet<string> validAgentNames,List<ValidationError> errors)
        {
            var nonNullMembers = new List<string>();

            if (selection.RoundRobinSelection != null)
                nonNullMembers.Add("round-robin-selection");

            if (selection.PromptSelect != null)
                nonNullMembers.Add("prompt-select");

            if (selection.SequentialSelection != null)
                nonNullMembers.Add("sequential-selection");

            if (nonNullMembers.Count > 1)
            {
                errors.Add(new ValidationError(
                    $"Only one selection type may be specified, but found multiple: {string.Join(", ", nonNullMembers)}.",
                    location
                ));
            }

            ValidateAgents(selection, location, validAgentNames, errors);

        }

        private static void ValidateAgents(YamlSelectionConfig selection, string location, HashSet<string> validAgentNames, List<ValidationError> errors)
        {
            // Validate SequentialSelection's InitialAgent
            if (selection.SequentialSelection?.InitialAgent is string seqAgent && !string.IsNullOrWhiteSpace(seqAgent))
            {
                ValidateAgentName(seqAgent, validAgentNames, $"{location}.sequential-selection.initial-agent", errors);
            }

            // Validate RoundRobinSelection's InitialAgent
            if (selection.RoundRobinSelection?.InitialAgent is string rrAgent && !string.IsNullOrWhiteSpace(rrAgent))
            {
                ValidateAgentName(rrAgent, validAgentNames, $"{location}.round-robin-selection.initial-agent", errors);
            }

            // Validate RoundRobinSelection's Agents list
            if (selection.RoundRobinSelection?.Agents is List<string> rrAgents)
            {
                for (int i = 0; i < rrAgents.Count; i++)
                {
                    var agentName = rrAgents[i];
                    if (!string.IsNullOrWhiteSpace(agentName))
                    {
                        ValidateAgentName(agentName, validAgentNames, $"{location}.round-robin-selection.agents[{i}]", errors);
                    }
                }
            }
        }

        private static void ValidateAgentName(string agentName,HashSet<string> validAgentNames, string location,List<ValidationError> errors)
        {
            if (!validAgentNames.Contains(agentName))
            {
                errors.Add(new ValidationError(
                    $"Unknown agent name '{agentName}'. Must be one of: {string.Join(", ", validAgentNames)}",
                    location
                ));
            }
        }


    }
}
