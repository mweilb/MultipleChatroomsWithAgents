

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using SemanticKernelExtension.Orchestrator;
using SemanticKernelExtension.Agents;
using YamlConfigurations;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using Microsoft.SemanticKernel.Agents.Chat;
using SemanticKernelExtension.AgentGroupChats.Strategies.Terminations;
 

#pragma warning disable SKEXP0110
namespace AppExtensions.Experience.Factories
{
    public static class AgentGroupChatOrchestratorFactory
    {
        public static async Task<AgentGroupChatOrchestrator?> Create(YamlMultipleChatRooms? experience, Kernel kernel)
        {
            // Safety check
            if (experience == null)
            {
                Console.WriteLine("Experience is null; cannot proceed.");
                return null;
            }

            // Check for rooms
            if (experience.Rooms == null || experience.Rooms.Count == 0)
            {
                Console.WriteLine("No rooms found in the YAML configuration.");
                return null;
            }
            
            AgentGroupChatOrchestrator orchestrator = new();

            // Iterate over each room
            foreach (var (roomName, roomConfig) in experience.Rooms)
            {
                Console.WriteLine($"\nRoom: {roomName}");

                // List out the agents in the room
                List<ChatHistoryKernelAgent> completionAgents = AgentsFactory.Create(experience, kernel, roomName, roomConfig);

                List<RuleBasedDefinition> listSKRules = RuleBasedDefinitionsFactory.Create(kernel, roomConfig, completionAgents);

                var groupChat = new AgentGroupChat([.. completionAgents])
                {
                    ExecutionSettings = new RuleBasedSettings(listSKRules)
                };

                orchestrator.Add(roomName, groupChat);
            }

            if (string.IsNullOrWhiteSpace(experience.StartRoom) == false)
            {
                orchestrator.SwitchTo(experience.StartRoom);
            }
            else
            {
                orchestrator.SwitchTo(experience.Rooms.First().Key);
            }

                // Check for auto-start rooms

                // Eventually you’ll construct and return real orchestrators here.
                // For now, returning null as requested.
                return await Task.FromResult<AgentGroupChatOrchestrator?>(orchestrator);
        }

    
 
       
    }
}
