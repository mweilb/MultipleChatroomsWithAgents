﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class SelectionValidation : IValidationPass
    {
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {
            if (errors is not IList<ValidationError> errorList)
                throw new ArgumentException("errors must be a mutable collection");

            if (config.Rooms == null)
            {
                return;
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
  
                        ValidateSelections(globalTerm, validAgentNames, errorList);


                        foreach (var rule in room.Strategies.Rules)
                        {
                             if (rule.Selection is YamlSelectionConfig ruleTerm)
                            {
                                if (ruleTerm != globalTerm)
                                {
                                    ValidateSelections(ruleTerm, validAgentNames, errorList);
                                }
                            }
                        }

                    }
                    else if (room.Strategies.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            if (rule.Selection is YamlSelectionConfig ruleTerm)
                            {
                                ValidateSelections(ruleTerm, validAgentNames, errorList);
                            }
                        }
                    }
                     
   
                }
            }

            // No return, mutate errorList in place
        }



        private void ValidateSelections(YamlSelectionConfig selection,  HashSet<string> validAgentNames, IList<ValidationError> errors)
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
    selection
));
            }

            ValidateAgents(selection, validAgentNames, errors);

        }

        private static void ValidateAgents(YamlSelectionConfig selection, HashSet<string> validAgentNames, IList<ValidationError> errors)
        {
            // Validate SequentialSelection's InitialAgent
            if (selection.SequentialSelection?.InitialAgent is string seqAgent && !string.IsNullOrWhiteSpace(seqAgent))
            {
                ValidateAgentName(seqAgent, validAgentNames, errors, selection.SequentialSelection);
            }

            // Validate RoundRobinSelection's InitialAgent
            if (selection.RoundRobinSelection?.InitialAgent is string rrAgent && !string.IsNullOrWhiteSpace(rrAgent))
            {
                ValidateAgentName(rrAgent, validAgentNames, errors, selection.RoundRobinSelection);
            }

            // Validate RoundRobinSelection's Agents list
            if (selection.RoundRobinSelection?.Agents is List<string> rrAgents)
            {
                for (int i = 0; i < rrAgents.Count; i++)
                {
                    var agentName = rrAgents[i];
                    if (!string.IsNullOrWhiteSpace(agentName))
                    {
                        ValidateAgentName(agentName, validAgentNames, errors, selection.RoundRobinSelection);
                    }
                }
            }
        }

        private static void ValidateAgentName(string agentName, HashSet<string> validAgentNames,IList<ValidationError> errors, YamlLineInfo lineInfo)
        {
            if (!validAgentNames.Contains(agentName))
            {
errors.Add(new ValidationError(
    $"Unknown agent name '{agentName}'. Must be one of: {string.Join(", ", validAgentNames)}",
    lineInfo
));
            }
        }


    }
}
