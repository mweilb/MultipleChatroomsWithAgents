﻿ 
namespace YamlConfigurations.Validations
{
    public class AgentReferenceValidation : IValidationPass
    {

        bool IsTrue(string? value)
        {
            return value != null &&
                   (value.Contains("yes", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("true", StringComparison.OrdinalIgnoreCase));
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

        
        public void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors)
        {
            if (errors is not IList<ValidationError> errorList)
                throw new ArgumentException("errors must be a mutable collection");

            var errorKeys = new HashSet<string>();

   

            if (config.Rooms != null)
            {
                var validRoomNames = config.Rooms.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var startRoomName = config.StartRoom?.Value ?? string.Empty;
                if ((config.StartRoom != null) && !string.IsNullOrEmpty(startRoomName) && !validRoomNames.Contains(startRoomName))
                {
                    errors.Add(new ValidationError(
                        $"StartRoom is not a valid room: '{config.StartRoom}'",
                        "[StartRoom]",
                        config.StartRoom
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
                                    errors.Add(new ValidationError(
                                        $"Current reference '{current.Name}' is not a valid 'user' keyword, agent in '{roomName}', or another room name.",
                                        $"[Room:{roomName}][Rule:{rule.Name}][Current:{current.Name}]",
                                        current));
                                }
                            }

                            foreach (var next in rule.Next)
                            {
                                if (!IsValidReference(next.Name, validAgentNames, validRoomNames, validTerminationNames))
                                {
                                    errors.Add(new ValidationError(
                                        $"Next reference '{next.Name}' is not a valid agent or room in room '{roomName}'.",
                                        $"[Room:{roomName}][Rule:{rule.Name}][Next:{next.Name}]",
                                        next));
                                }

                                if (validTerminationNames != null && validTerminationNames.Contains(next.Name))
                                {
                                    errors.Add(new ValidationError(
                                        $"Rooms[{roomName}].Strategies.Rule[{rule.Name}].Next[{next.Name}]",
                                        $"[Room:{roomName}][Rule:{rule.Name}][Next:{next.Name}]",
                                        next));
  
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
                                            errors.Add(new ValidationError(
                                            $"ContinuationAgentName '{termination.ContinuationAgentName}' is not reference in any rule.",
                                            $"[Room:{roomName}][Rule:{rule.Name}][Termination]",
                                            termination));
                                           
                                        }
                                    }
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
