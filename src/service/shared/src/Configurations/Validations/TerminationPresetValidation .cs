using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace MultiAgents.Configurations.Validations
{
    public class TerminationPresetValidation : IValidationPass
    {
        // Pattern for "yes" or "no"
        private static readonly Regex SimplePattern = new Regex(@"^(yes|no)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Existing colon-based patterns.
        // Single agent using colon and curly braces.
        private static readonly Regex SingleAgentPattern = new Regex(@"^(Not\s+)?Agent Name:\s*\{([^{}]+)\}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        // Multiple agents using colon and square brackets.
        private static readonly Regex MultipleAgentsPattern = new Regex(@"^(Not\s+)?Agent Name:\s*\[([^\[\]]+(,[^\[\]]+)*)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // New equals-based patterns.
        // Single agent: AgentName=Agent2, with optional "Not" prefix.
        private static readonly Regex SingleAgentEqualsPattern = new Regex(@"^(Not\s+)?AgentName\s*=\s*([^=\[\]]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        // Multiple agents: AgentNames=[Agent1, Agent2, ...], with optional "Not" prefix.
        private static readonly Regex MultipleAgentsEqualsPattern = new Regex(@"^(Not\s+)?AgentNames\s*=\s*\[([^\[\]]+(,[^\[\]]+)*)\]$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
                    : new HashSet<string>();

                if (room.Strategies != null)
                {
                    // If a global termination is defined, validate it and compare with child rules.
                    if (room.Strategies.GlobalTermination is YamlTerminationDecisionConfig globalTerm)
                    {
                        string globalLocation = $"Rooms[{roomName}].Strategies.GlobalTermination.Termination";
                        // Validate the global termination presets.
                        ValidateTerminationPresets(globalTerm.Presets, globalLocation, validAgentNames, errors);

                        if (room.Strategies.Rules != null)
                        {
                            foreach (var rule in room.Strategies.Rules)
                            {
                                if (rule.Termination is YamlTerminationDecisionConfig ruleTerm)
                                {
                                    if (ArePresetsEqual(globalTerm.Presets, ruleTerm.Presets))
                                    {
                                        // Mark that at least one child uses the identical global termination.
                                        
                                        // Skip validating this rule's termination presets.
                                    }
                                    else
                                    {
                                        string ruleLocation = $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Termination";
                                        ValidateTerminationPresets(ruleTerm.Presets, ruleLocation, validAgentNames, errors);
                                    }
                                }
                            }
                        }
 
                    }
                    else
                    {
                        // If no global termination is defined, validate each rule's termination presets.
                        if (room.Strategies.Rules != null)
                        {
                            foreach (var rule in room.Strategies.Rules)
                            {
                                if (rule.Termination is YamlTerminationDecisionConfig ruleTerm)
                                {
                                    string ruleLocation = $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Termination";
                                    ValidateTerminationPresets(ruleTerm.Presets, ruleLocation, validAgentNames, errors);
                                }
                            }
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Compares two lists of presets for equality, ignoring case and leading/trailing whitespace.
        /// </summary>
        private bool ArePresetsEqual(List<string> presets1, List<string> presets2)
        {
            if (presets1 == null && presets2 == null)
                return true;
            if (presets1 == null || presets2 == null)
                return false;
            if (presets1.Count != presets2.Count)
                return false;

            for (int i = 0; i < presets1.Count; i++)
            {
                if (!string.Equals(presets1[i].Trim(), presets2[i].Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        private void ValidateTerminationPresets(List<string> presets, string location, HashSet<string> validAgentNames, List<ValidationError> errors)
        {
            if (presets == null || presets.Count == 0)
            {
                return;
            }

            foreach (var preset in presets)
            {
                var trimmedPreset = preset.Trim();

                if (SimplePattern.IsMatch(trimmedPreset))
                {
                    // Valid simple preset ("yes" or "no"); nothing further to validate.
                    continue;
                }
                else if (SingleAgentPattern.IsMatch(trimmedPreset))
                {
                    // Colon-based single agent preset.
                    var match = SingleAgentPattern.Match(trimmedPreset);
                    var agentName = match.Groups[2].Value.Trim();
                    if (!validAgentNames.Contains(agentName))
                    {
                        errors.Add(new ValidationError(
                            $"Invalid termination preset '{preset}'. Agent name '{agentName}' is not valid in this room.",
                            location));
                    }
                }
                else if (MultipleAgentsPattern.IsMatch(trimmedPreset))
                {
                    // Colon-based multiple agents preset.
                    var match = MultipleAgentsPattern.Match(trimmedPreset);
                    var agentsGroup = match.Groups[2].Value;
                    var agentNames = agentsGroup.Split(',')
                                                .Select(name => name.Trim())
                                                .Where(name => !string.IsNullOrEmpty(name));
                    foreach (var agentName in agentNames)
                    {
                        if (!validAgentNames.Contains(agentName))
                        {
                            errors.Add(new ValidationError(
                                $"Invalid termination preset '{preset}'. Agent name '{agentName}' is not valid in this room.",
                                location));
                        }
                    }
                }
                else if (SingleAgentEqualsPattern.IsMatch(trimmedPreset))
                {
                    // Equals-based single agent preset.
                    var match = SingleAgentEqualsPattern.Match(trimmedPreset);
                    var agentName = match.Groups[2].Value.Trim();
                    if (!validAgentNames.Contains(agentName))
                    {
                        errors.Add(new ValidationError(
                            $"Invalid termination preset '{preset}'. Agent name '{agentName}' is not valid in this room.",
                            location));
                    }
                }
                else if (MultipleAgentsEqualsPattern.IsMatch(trimmedPreset))
                {
                    // Equals-based multiple agents preset.
                    var match = MultipleAgentsEqualsPattern.Match(trimmedPreset);
                    var agentsGroup = match.Groups[2].Value;
                    var agentNames = agentsGroup.Split(',')
                                                .Select(name => name.Trim())
                                                .Where(name => !string.IsNullOrEmpty(name));
                    foreach (var agentName in agentNames)
                    {
                        if (!validAgentNames.Contains(agentName))
                        {
                            errors.Add(new ValidationError(
                                $"Invalid termination preset '{preset}'. Agent name '{agentName}' is not valid in this room.",
                                location));
                        }
                    }
                }
                else
                {
                    errors.Add(new ValidationError(
                        $"Invalid termination preset '{preset}'. Valid options are: 'yes', 'no', " +
                        "'Agent Name: {{valid agent name}}', 'Not Agent Name: {{valid agent name}}', " +
                        "'Agent Name: [valid agent name, valid agent name2, ...]', 'Not Agent Name: [valid agent name, valid agent name2, ...]', " +
                        "'AgentName=valid agent name', 'Not AgentName=valid agent name', " +
                        "'AgentNames=[valid agent name, valid agent name2, ...]', or 'Not AgentNames=[valid agent name, valid agent name2, ...]'.",
                        location));
                }
            }
        }
    }
}
