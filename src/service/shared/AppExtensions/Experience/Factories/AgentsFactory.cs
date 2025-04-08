using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using SemanticKernelExtension.Agents;
using YamlConfigurations;

namespace AppExtensions.Experience.Factories
{
    public class AgentsFactory
    {
        public static List<ChatHistoryKernelAgent> Create(YamlMultipleChatRooms experience, Kernel kernel, string roomName, YamlRoomConfig roomConfig)
        {
            List<ChatHistoryKernelAgent> completionAgents = [];

            if (roomConfig.Agents != null && roomConfig.Agents.Any())
            {
                Console.WriteLine("  Agents:");
                foreach (var agent in roomConfig.Agents)
                {

                    if (agent.Instructions != null)
                    {
                        completionAgents.Add(
                            new ChatCompletionAgent()
                            {
                                Name = agent.Name,
                                Instructions = agent.Instructions,
                                Kernel = kernel
                            });
                    }
                    else if (agent.Echo != null)
                    {
                        completionAgents.Add(new EchoAgent(agent.Name, agent.Name, "Echo", agent.Echo, true));         
                    }
                    
                    Console.WriteLine($"    • Agent Name: {agent.Name}, Model: {agent.Model}, Emoji: {agent.Emoji}");
                }

                //Need to add all the rooms as they echo agents
                foreach (var room in experience.Rooms ?? [])
                {
                    var otherRoomNames = room.Key;
                    if (otherRoomNames != roomName)
                    {
                        completionAgents.Add(new RoomAgent(otherRoomNames, roomName, "room", otherRoomNames, false, kernel));
                    }
                }

            }
            else
            {
                Console.WriteLine("  (No agents found in this room.)");
            }

            return completionAgents;
        }
    }
}
