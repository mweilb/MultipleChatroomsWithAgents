using AppExtensions.Experience.Factories;
using AppExtensions.Experience.Handlers;
using Microsoft.SemanticKernel;
using SemanticKernelExtension.Orchestrator;
using WebSocketMessages;
using YamlConfigurations;

using YamlConfigurations.Librarians;
using static AppExtensions.Experience.ExperienceManager;

namespace AppExtensions.Experience
{
    public class ExperienceManager
    {
        public bool IsProcessingRelevantWebSocketMessage =>
            Experiences.Values.Any(t => t.handler != null && t.handler.IsProcessingRelevantMessage);
        public Kernel Kernel { get; }

        public class VisualInfo
        {
            public string Emoji = string.Empty;
            public string DisplayName = string.Empty;
        }

        public class TrackingInfo
        {
            public YamlMultipleChatRooms? Experience = null;
            public YamLibrarians? Librarians = null;
            public AgentGroupChatOrchestrator? agentGroupChatOrchestrator = null;
            public Dictionary<string, Dictionary<string, VisualInfo>>? VisualInfoPerName = null;
            public MessageHandler? handler = null;
        }

        public Dictionary<string, TrackingInfo> Experiences = [];

        private readonly RoomsHandler RoomsHandler;
        private readonly LibrarianHandler LibrarianHandler;

        public ExperienceManager(Kernel kernel)
        {
            Kernel = kernel;
            RoomsHandler = new RoomsHandler(this);
            LibrarianHandler = new LibrarianHandler(this.Experiences, kernel);
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

        /// <summary>
        /// Updates the Experiences dictionary to add or update rooms from the provided config.
        /// Removal of rooms is handled externally.
        /// </summary>
        public async Task UpdateFromConfigAsync(Dictionary<string, YamlMultipleChatRooms> config)
        {
            // Add or update rooms
            foreach (var kvp in config)
            {
                string roomName = kvp.Key;
                var updatedRoom = kvp.Value;
                if (updatedRoom == null) continue;

                // Only update if not present or changed (reference or simple equality)
                if (!Experiences.TryGetValue(roomName, out var tracking) ||
                    !Equals(tracking.Experience, updatedRoom))
                {
                    var trackingInfo = new TrackingInfo { Experience = updatedRoom };
                    var (orchestrator, visualInfo) = await AgentGroupChatOrchestratorFactory.Create(updatedRoom, Kernel);
                    trackingInfo.agentGroupChatOrchestrator = orchestrator;
                    trackingInfo.VisualInfoPerName = visualInfo;
                    Experiences[roomName] = trackingInfo;
                }
            }
        }



        public async Task<bool> CreateOrchestratorsAsync()
        {
            foreach (var kvp in Experiences)
            {
                var trackingInfo = kvp.Value;
                if (trackingInfo.agentGroupChatOrchestrator == null || trackingInfo.VisualInfoPerName == null)
                {
                    var (orchestrator, visualInfo) = await AgentGroupChatOrchestratorFactory.Create(trackingInfo.Experience, Kernel);
                    trackingInfo.agentGroupChatOrchestrator = orchestrator;
                    trackingInfo.VisualInfoPerName = visualInfo;


                }
            }
            return true;
        }

        public void RegisterHandlers(WebSocketHandler webSocketHandler)
        {
            webSocketHandler.RegisterCommand("rooms", RoomsHandler.HandleRoomsCommandAsync);
            webSocketHandler.RegisterCommand("librarians", LibrarianHandler.HandleLibrariansCommandAsync);

            foreach (var (key, group) in Experiences)
            {
                if (group != null)
                {
                    group.handler = new MessageHandler(group, key);
                    webSocketHandler.RegisterCommand(key, group.handler.HandleCommandAsync);
                    webSocketHandler.RegisterCommand(key + "-change-room", group.handler.HandleChangeRoomRequestAsync);
                }
            }


        }


    }
}
