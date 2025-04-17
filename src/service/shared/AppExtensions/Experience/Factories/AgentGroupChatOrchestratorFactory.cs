

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using SemanticKernelExtension.AgentGroupChats.Strategies.Terminations;
using SemanticKernelExtension.Agents;
using SemanticKernelExtension.Orchestrator;
using System.Reflection;
using YamlConfigurations;
 

#pragma warning disable SKEXP0110
namespace AppExtensions.Experience.Factories
{
    public static class AgentGroupChatOrchestratorFactory
    {
        public static async Task<(AgentGroupChatOrchestrator? Orchestrator, Dictionary<string, Dictionary<string, string>> RoomAgentEmojis)> Create(
            YamlMultipleChatRooms? experience, Kernel kernel)
        {
            // Safety check
            if (experience == null)
            {
                Console.WriteLine("Experience is null; cannot proceed.");
                return (null, new Dictionary<string, Dictionary<string, string>>());
            }

            // Check for rooms
            if (experience.Rooms == null || experience.Rooms.Count == 0)
            {
                Console.WriteLine("No rooms found in the YAML configuration.");
                return (null, new Dictionary<string, Dictionary<string, string>>());
            }

            AgentGroupChatOrchestrator orchestrator = new()
            {
                Name = experience.Name,
                LoggerFactory = kernel.Services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance,
            };


            var roomAgentEmojis = new Dictionary<string, Dictionary<string, string>>();

            // Iterate over each room
            foreach (var (roomName, roomConfig) in experience.Rooms)
            {
                Console.WriteLine($"\nRoom: {roomName}");

                // List out the agents in the room
                var (completionAgents, agentIcons) = AgentsFactory.Create(experience, kernel, roomName, roomConfig);

                // Build agent name -> emoji dictionary for this room
                var agentNameToEmoji = new Dictionary<string, string>();
                foreach (var agent in agentIcons.Keys)
                {
                    // Use agent.Name if available, fallback to ToString()
                    var nameProp = agent.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    string agentName = nameProp?.GetValue(agent)?.ToString() ?? agent.ToString() ?? "";
                    agentNameToEmoji[agentName] = agentIcons[agent] ?? "";
                }
                roomAgentEmojis[roomName] = agentNameToEmoji;

                List<RuleBasedDefinition> listSKRules = RuleBasedDefinitionsFactory.Create(kernel, roomConfig, completionAgents);

                var factory = kernel.Services.GetService<ILoggerFactory>();
                factory ??= NullLoggerFactory.Instance;

                var groupChat = new AgentGroupChat([.. completionAgents])
                {
                    LoggerFactory = factory,
                    ExecutionSettings = new RuleBasedSettings(listSKRules, factory)
                };

                foreach(var agent in completionAgents)
                {
                    if (agent is RoomRuleBasedAgent roomAgent)
                    {
                        roomAgent.SetExecutionSettings(groupChat.ExecutionSettings as RuleBasedSettings);
                    }
                }




                orchestrator.Add(roomName, groupChat);
            }

            if (string.IsNullOrWhiteSpace(experience.StartRoom) == false)
            {
                orchestrator.SetStartRoom(experience.StartRoom);
            }
            else
            {
                orchestrator.SetStartRoom(experience.Rooms.First().Key);
            }

            // Return orchestrator and room-agent-emoji dictionary
            return await Task.FromResult((orchestrator, roomAgentEmojis));
        }

    
 
       
    }
}
