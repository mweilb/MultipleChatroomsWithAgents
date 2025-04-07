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

                //Need to add all the rooms as they echo agents
                foreach (var room in experience.Rooms ?? [])
                {
                    var otherRoomNames = room.Key;
                    if (otherRoomNames != roomName)
                    {
                        var roomAgent = new RoomAgent(otherRoomNames, roomName, "room", otherRoomNames, false, kernel);
                        completionAgents.Add(roomAgent);
                        agentIcons[roomAgent] = ""; // No emoji/icon for RoomAgent
                    }
                }
            }
            else
            {
                Console.WriteLine("  (No agents found in this room.)");
            }

            return (completionAgents, agentIcons);
        }
    }
}
