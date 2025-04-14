 
using System.Text;
using System.Text.RegularExpressions;
using YamlConfigurations;

namespace YamlConfigurations.FileReader
{

    public static class MermaidGenerator
    {

        /// <summary>
        /// A small structure representing a Mermaid edge with a style-group.
        /// </summary>
        public class DiagramEdge
        {
            /// <summary>
            /// The line of Mermaid syntax, e.g. "A --> B"
            /// </summary>
            public string EdgeDefinition { get; set; } = string.Empty;

            /// <summary>
            /// A style group/category, e.g. "redStyle" or "blackStyle" or "withinRoom"
            /// </summary>
            public string StyleGroup { get; set; } = string.Empty;
        }

        // Extra helper class for tracking terminations
        public class TerminationUsage
        {
            public string Name { get; set; } = string.Empty;
            public YamlTerminationDecisionConfig? Termination { get; set; }
            public List<string> RuleNames { get; set; } = new List<string>();
        }

        public static string GenerateMermaidDiagram(YamlMultipleChatRooms chatRooms)
        {
            var sb = new StringBuilder();
            sb.AppendLine("graph TD");

            // 1. Define node styles (example)
            sb.AppendLine("classDef userStyle fill:#8FBC8F,stroke:#333,stroke-width:2px;");
            sb.AppendLine("classDef moderatorStyle fill:#FF8C00,stroke:#FF8C00,stroke-width:2px;");

            // If no rooms, just return what we have so far
            if (chatRooms.Rooms == null || chatRooms.Rooms.Count == 0)
                return sb.ToString();

            // 2. Gather any termination info needed (optional, see below)
            var terminationsByRoom = GatherTerminationsByRoom(chatRooms);

            // 3. We'll keep track of agent IDs per room
            var agentsPerRoom = new Dictionary<string, List<string>>();

            // 4. Build subgraphs for each room (with agents, moderator, terminations)
            BuildSubgraphs(chatRooms, sb, agentsPerRoom, terminationsByRoom);

            // 5. Build the edges
            //    We use DiagramEdge objects to hold both the edge definition
            //    and a style "group" label (like "withinStyle" or "crossStyle").
            var allEdges = new List<DiagramEdge>();

            // 5a. Within-room edges
            allEdges.AddRange(BuildWithinRoomEdges(chatRooms, agentsPerRoom));

            // 5b. Cross-room edges
            allEdges.AddRange(BuildCrossRoomEdges(chatRooms, agentsPerRoom));

            // 6. Render the edges (in the order we collected them)
            foreach (var edgeItem in allEdges)
            {
                sb.AppendLine(edgeItem.EdgeDefinition);
            }

            // 7. Generate linkStyle lines for each style group
            //    We'll group the edges by their style group
            var indexedEdges = allEdges
                .Select((edge, idx) => new { Index = idx, Edge = edge })
                .ToList();

            var groups = indexedEdges
                .GroupBy(x => x.Edge.StyleGroup);

            // Example logic: "withinStyle" might be red, "crossStyle" black, etc.
            // Adjust as desired.
            foreach (var group in groups)
            {
                // Build a comma-separated list of indices
                string indexList = string.Join(",", group.Select(g => g.Index));

                // Decide the style based on group.Key
                // You could do a switch or dictionary lookup:
                if (group.Key == "withinStyle")
                {
                    sb.AppendLine($"linkStyle {indexList} stroke-width:2px,fill:none,stroke:red;");
                }
                else if (group.Key == "crossStyle")
                {
                    sb.AppendLine($"linkStyle {indexList} stroke-width:2px,fill:none,stroke:black;");
                }
                else
                {
                    // fallback style
                    sb.AppendLine($"linkStyle {indexList} stroke-width:2px,fill:none,stroke:blue;");
                }
            }

            // 8. (Optional) Connect a global "Entry" node if needed
            string? globalStart = GetGlobalStartAgentId(chatRooms);
            if (!string.IsNullOrWhiteSpace(globalStart))
            {
                sb.AppendLine($"Entry -->|start| {globalStart}");
            }

            return sb.ToString();
        }


        #region Termination Gathering

        private static Dictionary<string, Dictionary<string, TerminationUsage>> GatherTerminationsByRoom(
            YamlMultipleChatRooms chatRooms)
        {
            var result = new Dictionary<string, Dictionary<string, TerminationUsage>>();
            if (chatRooms.Rooms == null) return result;

            foreach (var roomEntry in chatRooms.Rooms)
            {
                var room = roomEntry.Value;
                if (room?.Strategies?.Rules == null) continue;

                string roomId = GenerateRoomId(roomEntry.Key);
                int counter = 0;

                foreach (var rule in room.Strategies.Rules)
                {
                    if (rule.Termination != null)
                    {
                        counter++;

                        // If rule.next is pointing to a room (not an agent) and there is only one of them, skip this one
                        bool nextPointsToRoom = false;
                        if (rule.Next != null && rule.Next.Count == 1)
                        {
                            var nextObj = rule.Next[0];
                            if (nextObj != null)
                            {
                                string nextName = ((dynamic)nextObj).Name ?? "";
                                // If the nextName matches a room in chatRooms, it's a room
                                if (!string.IsNullOrEmpty(nextName) && chatRooms.Rooms != null && chatRooms.Rooms.ContainsKey(nextName))
                                {
                                    nextPointsToRoom = true;
                                }
                            }
                        }
                        // Count how many rules in this room have next pointing to a room
                        int rulesWithNextPointingToRoom = room.Strategies.Rules.Count(r =>
                        {
                            if (r.Next != null && r.Next.Count == 1)
                            {
                                var nextObj = r.Next[0];
                                if (nextObj != null)
                                {
                                    string nextName = ((dynamic)nextObj).Name ?? "";
                                    return !string.IsNullOrEmpty(nextName) && chatRooms.Rooms != null && chatRooms.Rooms.ContainsKey(nextName);
                                }
                            }
                            return false;
                        });

                        if (nextPointsToRoom && rulesWithNextPointingToRoom == 1)
                        {
                            // Skip this termination
                            continue;
                        }

                        if (string.IsNullOrEmpty(rule.Termination.ContinuationAgentName))
                            rule.Termination.ContinuationAgentName = $"Unnamed {counter}";

                        string terminationNodeId = GenerateTerminationId(room.Name, rule.Termination.ContinuationAgentName);

                        if (!result.TryGetValue(roomId, out var terminationsForRoom))
                        {
                            terminationsForRoom = new Dictionary<string, TerminationUsage>();
                            result[roomId] = terminationsForRoom;
                        }

                        if (!terminationsForRoom.TryGetValue(terminationNodeId, out var usage))
                        {
                            usage = new TerminationUsage
                            {
                                Termination = rule.Termination,
                                Name = rule.Termination.ContinuationAgentName
                            };
                            terminationsForRoom[terminationNodeId] = usage;
                        }

                        string ruleName = string.IsNullOrWhiteSpace(rule.Name)
                            ? $"Unnamed Rule {counter}"
                            : rule.Name;
                        usage.RuleNames.Add(ruleName);
                    }
                }
            }
            return result;
        }

        #endregion

        #region Building Subgraphs

        private static void BuildSubgraphs(
            YamlMultipleChatRooms chatRooms,
            StringBuilder sb,
            Dictionary<string, List<string>> agentsPerRoom,
            Dictionary<string, Dictionary<string, TerminationUsage>> terminationsByRoom)
        {
            if (chatRooms.Rooms == null) return;

            foreach (var roomEntry in chatRooms.Rooms)
            {
                string roomKey = roomEntry.Key;
                var room = roomEntry.Value;
                if (room == null) continue;

                string roomId = GenerateRoomId(roomKey);
                sb.AppendLine($"  subgraph {roomId}[{room.Emoji}{roomKey}]");

                bool needsStart = RoomRequiresStartNode(room);
                if (needsStart)
                    sb.AppendLine($"    start-{roomId}[start]");

                // Agents
                var agentIds = new List<string>();
                if (room.Agents != null)
                {
                    foreach (var agent in room.Agents)
                    {
                        string agentId = GenerateAgentId(roomId, agent.Name);
                        sb.AppendLine($"    {agentId}[{agent.Emoji}{agent.Name}]");
                        agentIds.Add(agentId);
                    }
                }
                agentsPerRoom[roomId] = agentIds;

                // Moderator
                if (room.Moderation != null)
                {
                    string moderatorId = GenerateAgentId(roomId, "Moderator");
                    sb.AppendLine($"    {moderatorId}[Moderator]:::moderatorStyle");
                }

                // Termination nodes
                if (terminationsByRoom.TryGetValue(roomId, out var terminationsForRoom))
                {
                    foreach (var termEntry in terminationsForRoom)
                    {
                        string terminationNodeId = termEntry.Key;
                        TerminationUsage usage = termEntry.Value;
                        sb.AppendLine($"    {terminationNodeId}((User Input: {usage.Name})):::userStyle");
                    }
                }

                sb.AppendLine("  end"); // end subgraph
            }
        }

        private static bool RoomRequiresStartNode(YamlRoomConfig room)
        {
            if (room?.Strategies?.Rules == null) return false;

            return room.Strategies.Rules.Any(rule =>
            {
                bool currentHasStart = rule.Current?.Any(a =>
                    a != null && string.Equals(((dynamic)a).Name, "start", StringComparison.OrdinalIgnoreCase)) ?? false;
                bool nextHasStart = rule.Next?.Any(a =>
                    a != null && string.Equals(((dynamic)a).Name, "start", StringComparison.OrdinalIgnoreCase)) ?? false;
                return currentHasStart || nextHasStart;
            });
        }

        #endregion

        #region Building Edges

        /// <summary>
        /// Creates DiagramEdge objects for connections within the same room.
        /// </summary>
        private static List<DiagramEdge> BuildWithinRoomEdges(
            YamlMultipleChatRooms chatRooms,
            Dictionary<string, List<string>> agentsPerRoom)
        {
            var edges = new List<DiagramEdge>();
            if (chatRooms.Rooms == null) return edges;

            foreach (var roomEntry in chatRooms.Rooms)
            {
                string roomKey = roomEntry.Key;
                var room = roomEntry.Value;
                string roomId = GenerateRoomId(roomKey);

                if (room?.Strategies?.Rules == null)
                    continue;

                int ruleRef = 0;
                foreach (var rule in room.Strategies.Rules)
                {
                    ruleRef++;
                    string ruleLabel = string.IsNullOrWhiteSpace(rule.Name)
                        ? $"Unnamed Rule {ruleRef}"
                        : rule.Name;

                    var currentAgents = GetRuleAgents(agentsPerRoom, roomId, rule.Current, room, chatRooms);
                    var nextAgents = GetRuleAgents(agentsPerRoom, roomId, rule.Next, room, chatRooms);

                    // Edge from each "current" to each "next" if they're in the same room
                    foreach (var cA in currentAgents)
                    {
                        foreach (var nA in nextAgents)
                        {
                            if (cA == nA) continue;
                            if (nA.StartsWith(roomId)  && (cA.StartsWith(roomId) || cA.StartsWith("start")))
                            {
                                edges.Add(new DiagramEdge
                                {
                                    EdgeDefinition = $"{cA} -->|{ruleLabel}| {nA}",
                                    StyleGroup = "withinStyle"
                                });
                            }
                        }
                    }

                    // Termination edges: nextAgent -> termination node
                    if (rule.Termination != null)
                    {
                        string termId = GenerateTerminationId(room.Name, rule.Termination.ContinuationAgentName ?? "");
                        foreach (var nextAgent in nextAgents)
                        {
                            if (nextAgent.StartsWith(roomId))
                            {
                                edges.Add(new DiagramEdge
                                {
                                    EdgeDefinition = $"{nextAgent} -->|{ruleLabel}| {termId}",
                                    StyleGroup = "withinStyle"
                                });
                            }
                        }
                    }
                }

                // Moderator -> Agents
                if (room?.Moderation != null && room?.Agents != null)
                {
                    string moderatorId = GenerateAgentId(roomId, "Moderator");
                    foreach (var agent in room.Agents)
                    {
                        string agentId = GenerateAgentId(roomId, agent.Name);
                        edges.Add(new DiagramEdge
                        {
                            EdgeDefinition = $"{moderatorId} -.-> {agentId}",
                            StyleGroup = "withinStyle"
                        });
                    }
                }
            }

            return edges;
        }

        /// <summary>
        /// Creates DiagramEdge objects for connections jumping from one room to another.
        /// </summary>
        private static List<DiagramEdge> BuildCrossRoomEdges(
            YamlMultipleChatRooms chatRooms,
            Dictionary<string, List<string>> agentsPerRoom)
        {
            var edges = new List<DiagramEdge>();
            if (chatRooms.Rooms == null) return edges;

            foreach (var roomEntry in chatRooms.Rooms)
            {
                string roomKey = roomEntry.Key;
                var room = roomEntry.Value;
                string roomId = GenerateRoomId(roomKey);

                if (room?.Strategies?.Rules == null)
                    continue;

                int ruleRef = 0;
                foreach (var rule in room.Strategies.Rules)
                {
                    ruleRef++;
                    string ruleLabel = string.IsNullOrWhiteSpace(rule.Name)
                        ? $"Unnamed Rule {ruleRef}"
                        : rule.Name;

                    var currentAgents = GetRuleAgents(agentsPerRoom, roomId, rule.Current, room, chatRooms);
                    var nextAgents = GetRuleAgents(agentsPerRoom, roomId, rule.Next, room, chatRooms);


                    foreach (var cA in currentAgents)
                    {
                        foreach (var nA in nextAgents)
                        {
                            // If nA doesn't start with roomId => different room
                            if (!nA.StartsWith(roomId))
                            {
                                edges.Add(new DiagramEdge
                                {
                                    EdgeDefinition = $"{cA} -->|{ruleLabel}| {nA}",
                                    StyleGroup = "crossStyle"
                                });
                            }
                        }
                    }
                }
            }
            return edges;
        }

        #endregion

        #region Identify a Global Start

        private static string? GetGlobalStartAgentId(YamlMultipleChatRooms chatRooms)
        {
            // Check if any rule references "start" in Current
            if (chatRooms.Rooms != null)
            {
                foreach (var roomEntry in chatRooms.Rooms)
                {
                    var room = roomEntry.Value;
                    string roomId = GenerateRoomId(roomEntry.Key);
                    if (room?.Strategies?.Rules == null) continue;

                    foreach (var rule in room.Strategies.Rules)
                    {
                        bool hasGlobalStart = rule.Current?.Any(a =>
                            a != null &&
                            string.Equals(((dynamic)a).Name, "start", StringComparison.OrdinalIgnoreCase)) ?? false;

                        if (hasGlobalStart)
                        {
                            return $"start-{roomId}";
                        }
                    }
                }

                // If no references to "start", pick first agent of the first room
                var firstPair = chatRooms.Rooms.FirstOrDefault();
                if (firstPair.Value?.Agents != null && firstPair.Value.Agents.Count > 0)
                {
                    string firstRoomKey = firstPair.Key;
                    var firstRoom = firstPair.Value;
                    string firstRoomId = GenerateRoomId(firstRoomKey);
                    return GenerateAgentId(firstRoomId, firstRoom.Agents[0].Name);
                }
            }
            return null;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Transforms rule "Current" or "Next" references into actual node IDs in the diagram.
        /// </summary>
        private static List<string> GetRuleAgents<T>(
            Dictionary<string, List<string>> agentsPerRoom,
            string roomId,
            IList<T>? ruleAgents,
            YamlRoomConfig currentRoom,
            YamlMultipleChatRooms allRooms)
        {
            if (ruleAgents == null || ruleAgents.Count == 0)
            {
                // If no specific agents listed => use all agents in this room
                return agentsPerRoom.TryGetValue(roomId, out var allAgents)
                    ? allAgents
                    : new List<string>();
            }

            // If "any" is present => all agents in this room
            bool hasAny = ruleAgents.Any(a =>
                a != null && string.Equals(((dynamic)a).Name, "any", StringComparison.OrdinalIgnoreCase));
            if (hasAny)
            {
                return agentsPerRoom.TryGetValue(roomId, out var allAgents)
                    ? allAgents
                    : new List<string>();
            }

            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var agentEntry in ruleAgents)
            {
                if (agentEntry == null) continue;
                string name = ((dynamic)agentEntry).Name ?? "";

                // "start" references the special start node for this room
                if (string.Equals(name, "start", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add($"start-{roomId}");
                    continue;
                }

                // If the agent name matches a known agent in this room
                var possibleAgentId = GenerateAgentId(roomId, name);
                if (agentsPerRoom.TryGetValue(roomId, out var knownAgents)
                    && knownAgents.Contains(possibleAgentId))
                {
                    result.Add(possibleAgentId);
                    continue;
                }

                // If it matches a termination in this room
                bool isTermination = currentRoom.Strategies?.Rules?.Any(r =>
                    r.Termination != null &&
                    string.Equals(r.Termination.ContinuationAgentName, name, StringComparison.OrdinalIgnoreCase)) ?? false;

                if (isTermination)
                {
                    string terminationNodeId = GenerateTerminationId(currentRoom.Name, name);
                    result.Add(terminationNodeId);
                    continue;
                }

                // Otherwise, treat as a reference to another room by name
                if (allRooms.Rooms != null && allRooms.Rooms.TryGetValue(name, out var targetRoom) && targetRoom != null)
                {
                    string targetRoomId = GenerateRoomId(name);
                    if (RoomRequiresStartNode(targetRoom))
                    {
                        result.Add($"start-{targetRoomId}");
                    }
                    else if (targetRoom.Agents != null && targetRoom.Agents.Count > 0)
                    {
                        // Link to the first agent in that room
                        string firstAgent = targetRoom.Agents[0].Name;
                        result.Add(GenerateAgentId(targetRoomId, firstAgent));
                    }
                    else
                    {
                        // Fallback: link to the subgraph itself
                        result.Add(targetRoomId);
                    }
                }
                else
                {
                    // If not found, fallback to generating a "fake" room ID
                    result.Add(GenerateRoomId(name));
                }
            }

            return result.ToList();
        }

        private static string GenerateTerminationId(string roomName, string terminationKey)
            => $"{SanitizeId($"Room_{roomName}")}_Termination_{SanitizeId(terminationKey)}";

        private static string GenerateRoomId(string roomKey)
            => $"Room_{SanitizeId(roomKey)}";

        private static string GenerateAgentId(string roomId, string agentName)
            => $"{roomId}_Agent_{SanitizeId(agentName)}";

        private static string SanitizeId(string input)
            => string.IsNullOrEmpty(input)
                ? string.Empty
                : Regex.Replace(input, @"[^\w]", "_");

        #endregion
    }
}
