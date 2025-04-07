using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using YamlConfigurations;

namespace YamlConfigurations.Validations
{
    public class TerminationValidation : IValidationPass
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
                    if (room.Strategies.GlobalTermination is YamlTerminationDecisionConfig globalTerm)
                    {
                        string globalLocation = $"Rooms[{roomName}].Strategies.GlobalTermination.Termination";

                        ValidateTerminations(globalTerm, globalLocation, validAgentNames, errors);

                        foreach (var rule in room.Strategies.Rules)
                        {
                            string ruleLocation = $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Termination";
                            if (rule.Termination is YamlTerminationDecisionConfig ruleTerm)
                            {
                                if (ruleTerm != globalTerm)
                                {
                                    ValidateTerminations(ruleTerm, ruleLocation, validAgentNames, errors);
                                }
                            }
                        }

                    }
                    else if (room.Strategies.Rules != null)
                    {
                        foreach (var rule in room.Strategies.Rules)
                        {
                            string ruleLocation = $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Termination.Termination";
                            if (rule.Termination is YamlTerminationDecisionConfig ruleTerm)
                            {
                                ValidateTerminations(ruleTerm, ruleLocation, validAgentNames, errors);
                            }
                        }
                    }
                     
   
                }
            }

            return errors;
        }

        private void ValidateTerminations(
                    YamlTerminationDecisionConfig termConfig,
                    string location,
                    HashSet<string> validAgentNames,
                    List<ValidationError> errors)
        {
            // Track which members are not null
            var nonNullMembers = new List<string>();

         
            if (termConfig.RegexTermination != null)
                nonNullMembers.Add("regex-termination");

            if (termConfig.ConstantTermination != null)
                nonNullMembers.Add("constant-termination");

            if (termConfig.PromptTermination != null)
                nonNullMembers.Add("prompt-termination");

            // If more than one is set, it's an error
            if (nonNullMembers.Count > 1)
            {
                errors.Add(new ValidationError(
                    $"Only one termination type may be specified, but found multiple: {string.Join(", ", nonNullMembers)}.",
                    location
                ));
            }

           
            
        }

    }
}
