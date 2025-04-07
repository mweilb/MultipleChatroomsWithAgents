using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace YamlConfigurations.FileReader
{
    // Helper class to track which rules use a given termination.
    public class TerminationUsage
    {
        public string Name = string.Empty;
        public YamlTerminationDecisionConfig? Termination { get; set; }
        public List<string> RuleNames { get; set; } = new List<string>();
    }

    public static class MermaidGenerator
    {
        public static string GenerateMermaidDiagram(YamlMultipleChatRooms chatRooms)
        {
            var sb = new StringBuilder();
            sb.AppendLine("graph TD");

            // Define node styles.
            sb.AppendLine("classDef userStyle fill:#8FBC8F,stroke:#333,stroke-width:2px;");          // green box for user
            sb.AppendLine("classDef moderatorStyle fill:#FF8C00,stroke:#FF8C00,stroke-width:2px;");      // dark orange for moderators

            if (chatRooms.Rooms == null || chatRooms.Rooms.Count == 0)
                return sb.ToString();

          

            // Build dictionaries for agents per room and termination usage.
            var agentsPerRoom = new Dictionary<string, List<string>>();
            var terminationsByRoom = GetTerminationsByRoom(chatRooms);

            // Process each room: add agent nodes and termination nodes.
            GetRoomsInfo(chatRooms, sb, agentsPerRoom, terminationsByRoom);

            // We'll collect edges in two separate lists to style them later.
            var agentEdges = new List<string>();
            var terminationEdges = new List<string>();
            var moderatorEdges = new List<string>();

            // Process each room's rules to create connections.
            foreach (var roomEntry in chatRooms.Rooms)
            {
                string roomId = GenerateRoomId(roomEntry.Key);
                var room = roomEntry.Value;
                if (room?.Agents == null)
                    continue;

                if (room.Strategies?.Rules?.Count > 0)
                {
                    int ruleRef = 0;
                    foreach (var rule in room.Strategies.Rules)
                    {
                        ruleRef++;
                        var currentAgents = GetCurrentAgents(agentsPerRoom, roomId, rule, room);
                        var nextAgents = GetNextAgents(agentsPerRoom, roomId, rule, room);
                        foreach (var currentId in currentAgents)
                        {
                            foreach (var nextId in nextAgents)
                            {
                                // Create an agent-to-agent edge with the rule name.
                                string edgeText = string.IsNullOrEmpty(rule.Name)
                                    ? $"{currentId} -->|Unnamed Rule {ruleRef}| {nextId}"
                                    : $"{currentId} -->|{rule.Name}| {nextId}";
                                agentEdges.Add("                " + edgeText);
                            }
                        }
                        // Create termination edges (only if the rule has a termination)
                        var termEdges = GetTerminationToNextAgentEdges(agentsPerRoom, roomId, room, rule);
                        terminationEdges.AddRange(termEdges);
                    }
                }

                if (room.Moderation != null)
                {
                    string moderatorId = GenerateAgentId(roomId + "_", "Moderator");
                    foreach (var roomAgent in room.Agents)
                    {
                        string agentId = GenerateAgentId(roomId, roomAgent.Name);
                        moderatorEdges.Add($"{moderatorId} -.-> {agentId}");
                    }
                 
                    
                }
            }

            // Append all agent-to-agent edges.
            foreach (var edge in agentEdges)
                sb.AppendLine(edge);
            // Append all termination edges.
            foreach (var edge in terminationEdges)
                sb.AppendLine(edge);
            foreach (var edge in moderatorEdges)
                sb.AppendLine(edge);

            // Determine the start room and connect the user node.
            var startRoomKey = string.IsNullOrWhiteSpace(chatRooms.StartRoom)
                ? chatRooms.Rooms?.FirstOrDefault().Key
                : chatRooms.StartRoom;

            if (!string.IsNullOrWhiteSpace(startRoomKey))
                sb.AppendLine($" Entry -->|start| {GenerateRoomId(startRoomKey)}");

            // Now output linkStyle directives.
            // The agent-to-agent edges will be styled blue.
            string agentEdgeIndices = string.Join(",", Enumerable.Range(0, agentEdges.Count));
            if (!string.IsNullOrEmpty(agentEdgeIndices))
                sb.AppendLine($"linkStyle {agentEdgeIndices} stroke:#0000FF,stroke-width:2px;");

            // The termination edges (which come after agentEdges) will be styled reddish.
            string terminationEdgeIndices = string.Join(",", Enumerable.Range(agentEdges.Count, terminationEdges.Count));
            if (!string.IsNullOrEmpty(terminationEdgeIndices))
                sb.AppendLine($"linkStyle {terminationEdgeIndices} stroke:#F08080,stroke-width:2px;");

            string moderatorEdgeIndices = string.Join(",", Enumerable.Range(agentEdges.Count + terminationEdges.Count, moderatorEdges.Count));
            if (!string.IsNullOrEmpty(moderatorEdgeIndices))
                sb.AppendLine($"linkStyle {moderatorEdgeIndices} stroke:#F08080,stroke-width:2px;");



            return sb.ToString();
        }

        // Returns termination edge strings for a given rule.
        // Edges are drawn from the "next" agent (if in the same room and not "user")
        // to the termination node. The edge is labeled with the rule that triggers the termination.
        private static List<string> GetTerminationToNextAgentEdges(Dictionary<string, List<string>> agentsPerRoom, string roomId, YamlRoomConfig room, YamlStratergyRules rule)
        {
            var edges = new List<string>();
            if (rule.Termination != null)
            {

                string terminationNodeId = GenerateTerminationId(room.Name, rule.Termination.ContinuationAgentName??"");
                var nextAgents = GetNextAgents(agentsPerRoom, roomId, rule, room);
                string ruleLabel = string.IsNullOrWhiteSpace(rule.Name) ? "Unnamed Rule" : rule.Name;
                foreach (var agentId in nextAgents)
                {
                    // Do not add a termination edge if the next agent is "user" or not in the same room.

                    if (agentId.Equals("start", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!agentId.StartsWith(roomId))
                        continue;
                    // Flip the edge: from the next agent to the termination node.
                    edges.Add($"                {agentId} -->|{ruleLabel}| {terminationNodeId}");
                }
            }
            return edges;
        }
 

        // Scans all rooms to capture unique termination configurations and the rules that use them.
        private static Dictionary<string, Dictionary<string, TerminationUsage>> GetTerminationsByRoom(YamlMultipleChatRooms chatRooms)
        {
            var result = new Dictionary<string, Dictionary<string, TerminationUsage>>();
            if (chatRooms.Rooms == null)
                return result;

            foreach (var roomEntry in chatRooms.Rooms)
            {
                string roomId = GenerateRoomId(roomEntry.Key);
                var room = roomEntry.Value;
                if (room?.Strategies?.Rules == null)
                    continue;

                int counter = 0;
                foreach (var rule in room.Strategies.Rules)
                {
                    if (rule.Termination != null)
                    {
                        counter++;
                        if (string.IsNullOrEmpty(rule.Termination.ContinuationAgentName))
                        {
                            rule.Termination.ContinuationAgentName = $"Unnamed {counter}";
                        }
                        string key = GenerateTerminationId(room.Name, rule.Termination.ContinuationAgentName);
                        if (!result.TryGetValue(roomId, out var terminations))
                        {
                            terminations = new Dictionary<string, TerminationUsage>();
                            result[roomId] = terminations;
                        }
                        if (!terminations.TryGetValue(key, out var usage))
                        {
                            
                            usage = new TerminationUsage { Termination = rule.Termination, Name = rule.Termination.ContinuationAgentName };
                            terminations[key] = usage;
                        }
                        // Use rule.Name if available; otherwise, fallback to "Unnamed Rule <counter>".
                        string ruleIdentifier = string.IsNullOrWhiteSpace(rule.Name) ? $"Unnamed Rule {counter}" : rule.Name;
                        usage.RuleNames.Add(ruleIdentifier);
                    }
                }
            }
            return result;
        }

        // Processes each room by adding agent nodes and termination nodes.
        private static void GetRoomsInfo(
            YamlMultipleChatRooms chatRooms,
            StringBuilder sb,
            Dictionary<string, List<string>> agentsPerRoom,
            Dictionary<string, Dictionary<string, TerminationUsage>> terminationsByRoom)
        {
            if (chatRooms.Rooms == null)
                return;

            foreach (var roomEntry in chatRooms.Rooms)
            {
                string roomId = GenerateRoomId(roomEntry.Key);
                var room = roomEntry.Value;
                if (room?.Agents == null || room.Agents.Count == 0)
                    continue;

                var agentIds = new List<string>();
                sb.AppendLine($"        subgraph {roomId}[{roomEntry.Value.Emoji + roomEntry.Key}]");

                sb.AppendLine($"   start-{roomId}[start]");
             

                // Add agent nodes.
                foreach (var roomAgent in room.Agents)
                {
                    string agentId = GenerateAgentId(roomId, roomAgent.Name);
                    agentIds.Add(agentId);
                    sb.AppendLine($"            {agentId}[{roomAgent.Emoji + roomAgent.Name}]");
                }

                // Add termination nodes for this room if any exist.
                if (terminationsByRoom.TryGetValue(roomId, out var terminations))
                {
                    foreach (var kvp in terminations)
                    {
                        string key = kvp.Key;
                        TerminationUsage usage = kvp.Value;
                        string terminationName = usage.Name;
                        // Label includes the rule names that reference this termination.
                        string ruleList = string.Join(", ", usage.RuleNames);
                        sb.AppendLine($"            {key}((User Input: {terminationName})):::userStyle");
                    }
                }

      
                if (room.Moderation != null)
                {
                    string moderatorId = GenerateAgentId(roomId+"_", "Moderator");
                    sb.AppendLine($"            {moderatorId}[Moderator]:::moderatorStyle");
                }

                sb.AppendLine("        end");

                agentsPerRoom[roomId] = agentIds;
            }
        }

      
        // Generic helper that returns a list of agent IDs based on the rule's agent lists.
        // Updated helpers in MermaidGenerator to handle termination names.
        private static List<string> GetCurrentAgents(
            Dictionary<string, List<string>> agentsPerRoom,
            string roomId,
            YamlStratergyRules rule,
            YamlRoomConfig room) =>
                GetRuleAgents(agentsPerRoom, roomId, rule?.Current, room);

        private static List<string> GetNextAgents(
            Dictionary<string, List<string>> agentsPerRoom,
            string roomId,
            YamlStratergyRules rule,
            YamlRoomConfig room) =>
                GetRuleAgents(agentsPerRoom, roomId, rule?.Next, room);

        private static List<string> GetRuleAgents<T>(
            Dictionary<string, List<string>> agentsPerRoom,
            string roomId,
            IList<T>? ruleAgents,
            YamlRoomConfig room)
        {
            // Retrieve available agents for the room.
            var availableAgents = agentsPerRoom.ContainsKey(roomId)
                ? agentsPerRoom[roomId]
                : new List<string>();

            // If no specific rule agents are defined, return all available agents.
            if (ruleAgents == null || ruleAgents.Count == 0)
            {
                return availableAgents.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            // If any rule agent has a name equal to "any", return all available agents.
            if (ruleAgents.Any(agent => agent != null &&
                string.Equals(((dynamic)agent).Name, "any", StringComparison.OrdinalIgnoreCase)))
            {
                return availableAgents.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            }

            // Build a list based on each rule agent, avoiding duplicates.
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var agent in ruleAgents)
            {
                if (agent == null)
                    continue;
                string name = ((dynamic)agent)?.Name ?? string.Empty;

                // If the agent name is "start", add the common user node.
                if (string.Equals(name, "start", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add($"start-{roomId}");
                }
                else
                {
                    // Generate an agentId for the given name.
                    string agentId = GenerateAgentId(roomId, name);
                    if (availableAgents.Contains(agentId))
                    {
                        result.Add(agentId);
                    }
                    else
                    {
                        // Instead of automatically treating the name as a room,
                        // check if this name corresponds to a termination defined in the room.
                        bool isTermination = room.Strategies?.Rules?.Any(r =>
                            r.Termination != null &&
                            string.Equals(r.Termination.ContinuationAgentName, name, StringComparison.OrdinalIgnoreCase)) ?? false;
                        if (isTermination)
                        {
                            // Get the first matching termination rule.
                            var terminationRule = room.Strategies?.Rules?.FirstOrDefault(r =>
                                r.Termination != null &&
                                string.Equals(r.Termination.ContinuationAgentName, name, StringComparison.OrdinalIgnoreCase));
                            if (terminationRule != null)
                            { 
                                if (terminationRule.Termination != null)
                                {
                                    string key = GenerateTerminationId(room.Name, terminationRule.Termination.ContinuationAgentName??"");
                              
                                    result.Add(key);
                                }
                            }
                            else
                            {
                                // Fallback: treat as room if no matching termination rule is found.
                                result.Add(GenerateRoomId(name));
                            }
                        }
                        else
                        {
                            // If not an agent or termination, assume it's a room reference.
                            result.Add(GenerateRoomId(name));
                        }
                    }
                }
            }

            return result.ToList();
        }


   

        private static string GenerateTerminationId(string roomName, string terminationKey) =>
            $"{SanitizeId(roomName)}_Termination_{SanitizeId(terminationKey)}";

        private static string GenerateRoomId(string roomKey) =>
            $"Room_{SanitizeId(roomKey)}";

        private static string GenerateAgentId(string roomId, string agentName) =>
            $"{roomId}_Agent_{SanitizeId(agentName)}";

        private static string SanitizeId(string input) =>
            string.IsNullOrEmpty(input) ? string.Empty : Regex.Replace(input, @"[^\w]", "_");
    }
}
