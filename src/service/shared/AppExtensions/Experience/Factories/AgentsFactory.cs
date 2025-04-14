using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using SemanticKernelExtension.Agents;
using YamlConfigurations;


#pragma warning disable SKEXP0110

namespace AppExtensions.Experience.Factories
{
    public class AgentsFactory
    {
        public static (List<ChatHistoryAgent> Agents, Dictionary<ChatHistoryAgent, string> AgentIcons) Create(
            YamlMultipleChatRooms experience, Kernel kernel, string roomName, YamlRoomConfig roomConfig)
        {
            List<ChatHistoryAgent> completionAgents = [];
            Dictionary<ChatHistoryAgent, string> agentIcons = new();

            if (roomConfig.Agents != null && roomConfig.Agents.Any())
            {
                var factory = kernel.Services.GetService<ILoggerFactory>();
                factory ??= NullLoggerFactory.Instance;

                Console.WriteLine("  Agents:");
                foreach (var agent in roomConfig.Agents)
                {
                    ChatHistoryAgent? createdAgent = null;

                    if (agent.Instructions != null)
                    {
                        createdAgent = new ChatCompletionAgent()
                        {
                            LoggerFactory = factory,
                            Name = agent.Name,
                            Instructions = agent.Instructions,
                            Kernel = kernel
                        };
                        completionAgents.Add(createdAgent);
                    }
                    else if (agent.Echo != null)
                    {
                        createdAgent = new EchoAgent(agent.Name, agent.Name, "Echo", agent.Echo, true) { LoggerFactory = factory };
                        completionAgents.Add(createdAgent);
                    }

                    if (createdAgent != null)
                    {
                        agentIcons[createdAgent] = agent.Emoji ?? "";
                    }

                    Console.WriteLine($"    • Agent Name: {agent.Name}, Model: {agent.Model}, Emoji: {agent.Emoji}");
                }

                // Find the parent room by key, or use the first as fallback
 
                // Add all other rooms as echo agents with rules
                foreach (var (otherRoomName, otherRoom) in experience.Rooms ?? [])
                {
                    if (otherRoomName == roomName) continue;

                    var roomAgent = new RoomRuleBasedAgent(otherRoomName, roomName, "room", otherRoomName, false, kernel, null);

                    completionAgents.Add(roomAgent);
                    agentIcons[roomAgent] = otherRoom.Emoji ?? "";

                    var strategies = roomConfig.Strategies?.Rules ?? [];
                    foreach (var rule in strategies)
                    {
                        foreach (var next in strategies.SelectMany(r => r.Next).Where(a => a.Name == otherRoomName))
                        {
                            if (next.Name != otherRoomName) { continue; }

                            string instructions = next.ContextTransfer?.Prompt ?? "Summarize the Conversations as concise as possible and dont try to answer questions.";
                            var yieldOnChange = IsTrue(next.ContextTransfer?.YieldOnRoomChange);
                            var yieldCanceledName = next.ContextTransfer?.YieldCanceledName ?? string.Empty;

                            roomAgent.InsertRuleInfo(roomName, rule.Name, instructions, yieldOnChange, yieldCanceledName);
                        }
                    }
                    
                }
                 
            }
            else
            {
                Console.WriteLine("  (No agents found in this room.)");
            }

            return (completionAgents, agentIcons);
        }

        public static bool IsTrue(string? value)
        {
            return value != null &&
                   (value.Contains("yes", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("true", StringComparison.OrdinalIgnoreCase));
        }

    }
}
