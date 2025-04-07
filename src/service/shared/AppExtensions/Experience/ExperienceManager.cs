using AppExtensions.Experience.Factories;
using AppExtensions.Experience.Handlers;
using Microsoft.SemanticKernel;
using SemanticKernelExtension.Orchestrator;
using WebSocketMessages;
using YamlConfigurations;
 
using YamlConfigurations.Librarians;

namespace AppExtensions.Experience
{
    public class ExperienceManager
    {
        public Kernel Kernel { get; }

        public class TrackingInfo {
            public YamlMultipleChatRooms? Experience = null;
            public YamLibrarians? Librarians = null;
            public AgentGroupChatOrchestrator? agentGroupChatOrchestrator = null;
        }

        public Dictionary<string, TrackingInfo> Experiences = [];
        
        private readonly RoomsHandler RoomsHandler;

        public ExperienceManager(Kernel kernel)
        {
            Kernel = kernel;
            RoomsHandler = new RoomsHandler(this);
        }

        public async Task<bool> ReadDirectoryAsync(string directory)
        {
            var experinces = ExperienceLoader.LoadExperiences(directory);
            foreach (var kvp in experinces)
            {
                if (!Experiences.ContainsKey(kvp.Key))
                {
                    var trackingInfo = new TrackingInfo();
                    trackingInfo.Experience = kvp.Value;
                    Experiences.Add(kvp.Key, trackingInfo);

                    // Await the asynchronous gathering of librarians.
                    trackingInfo.Librarians = await ExperienceLoader.GatherLibrariansAsync(kvp.Value, Kernel);
                }
            }

            return true;
        }


        public async Task<bool> CreateOrchestratorsAsync()
        {
            foreach (var kvp in Experiences)
            {
                var trackingInfo = kvp.Value;
                trackingInfo.agentGroupChatOrchestrator ??= await AgentGroupChatOrchestratorFactory.Create(trackingInfo.Experience, Kernel);
            }
            return true;
        }

        public void RegisterHandlers(WebSocketHandler webSocketHandler)
        {
            webSocketHandler.RegisterCommand("rooms", RoomsHandler.HandleRoomsCommandAsync);
        }

    }
}
