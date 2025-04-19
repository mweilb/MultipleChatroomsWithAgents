

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using SemanticKernelExtension.Agents;
using SemanticKernelExtension.Orchestrator;
using System.Reflection;
using YamlConfigurations;
using static AppExtensions.Experience.ExperienceManager;


#pragma warning disable SKEXP0110
namespace AppExtensions.Experience.Factories
{
    public static class AgentGroupChatOrchestratorFactory
    {
        public static async Task<(AgentGroupChatOrchestrator? Orchestrator, Dictionary<string, Dictionary<string, VisualInfo>> RoomAgentEmojis)> Create(
            YamlMultipleChatRooms? experience, Kernel kernel)
        {
            // Safety check
            if (experience == null)
            {
                Console.WriteLine("Experience is null; cannot proceed.");
                return (null, new Dictionary<string, Dictionary<string, VisualInfo>>());
            }

            // Check for rooms
            if (experience.Rooms == null || experience.Rooms.Count == 0)
            {
                Console.WriteLine("No rooms found in the YAML configuration.");
                return (null, new Dictionary<string, Dictionary<string, VisualInfo>>());
            }

            AgentGroupChatOrchestrator orchestrator = new()
            {
                Name = experience.Name,
                LoggerFactory = kernel.Services.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance,
            };


            var roomVisualInfo = new Dictionary<string, Dictionary<string, VisualInfo>>();

            // Iterate over each room
            foreach (var (roomName, roomConfig) in experience.Rooms)
            {
                Console.WriteLine($"\nRoom: {roomName}");

                // List out the agents in the room
                var (completionAgents, agentsVisualInfos) = AgentsFactory.Create(experience, kernel, roomName, roomConfig);

                // Build agent name -> emoji dictionary for this room
                var agentVisualInfo = new Dictionary<string, VisualInfo>();
                foreach (var (agent, visualInfo) in agentsVisualInfos)
                {
                    // Use agent.Name if available, fallback to ToString()
                    var nameProp = agent.GetType().GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
                    string agentName = nameProp?.GetValue(agent)?.ToString() ?? agent.ToString() ?? "";
                    agentVisualInfo[agentName] = visualInfo;          
                }
                roomVisualInfo[roomName] = agentVisualInfo;

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
            return await Task.FromResult((orchestrator, roomVisualInfo));
        }

    
 
       
    }
}
