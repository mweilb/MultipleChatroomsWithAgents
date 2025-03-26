using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgents.AgentsChatRoom.Rooms;
using MultiAgents.Configurations;
using MultiAgents.Configurations.FileReader;
using MultiAgents.Configurations.Librarians;
using MultiAgents.Configurations.Validations;
using MultiAgents.WebSockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace api.src
{
    public class RegisterYamls
    {
        // This helper method encapsulates the common logic for reading YAML files, validating, and registering rooms.
        private static async Task<(Dictionary<string, YamlMultipleChatRooms>, Dictionary<string, YamLibrarians>)> RegisterRoomsAsync(
            Kernel kernel,
            int embeddedSize,
            WebSocketHandler webSocketHandler,
            string directory,
            Func<string, Kernel, int, ILoggerFactory, Dictionary<string, YamlMultipleChatRooms>> readMethod)
        {
            // Get all YAML files (.yml and .yaml) in the specified directory.
            var yamlFiles = Directory.GetFiles(directory, "*.yml")
                                     .Union(Directory.GetFiles(directory, "*.yaml"));

            // Create a logger factory.
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var allRooms = new Dictionary<string, YamlMultipleChatRooms>();
            var allLibrarians = new Dictionary<string, YamLibrarians>();

            // Iterate over each YAML file and register its rooms.
            foreach (var yamlFilePath in yamlFiles)
            {
                var experienceDict = readMethod(yamlFilePath, kernel, embeddedSize, loggerFactory);

                foreach (var kvp in experienceDict)
                {
                    if (!allRooms.ContainsKey(kvp.Key))
                    {
                        allRooms.Add(kvp.Key, kvp.Value);
                        var librarians = new YamLibrarians();
                        // Await the asynchronous gathering of librarians.
                        if (await librarians.GatherLibrariansAsync(kvp.Value, kernel))
                        {
                            allLibrarians.Add(kvp.Key, librarians);
                        }
                    }
                }
            }
            return (allRooms, allLibrarians);
        }

        // Register single chat rooms using the ReadRoom method from YamlFileReader.
        public static Task<(Dictionary<string, YamlMultipleChatRooms>, Dictionary<string, YamLibrarians>)> RegisterSingleRoomsAsync(
            Kernel kernel,
            int embeddedSize,
            WebSocketHandler webSocketHandler,
            string agentsDirectory)
        {
            return RegisterRoomsAsync(kernel, embeddedSize, webSocketHandler, agentsDirectory, YamlFileReader.ReadRoom);
        }

        // Register multi chat rooms using the Read method from YamlFileReader.
        public static Task<(Dictionary<string, YamlMultipleChatRooms>, Dictionary<string, YamLibrarians>)> RegisterMultiRoomsAsync(
            Kernel kernel,
            int embeddedSize,
            WebSocketHandler webSocketHandler,
            string experiencesDirectory)
        {
            return RegisterRoomsAsync(kernel, embeddedSize, webSocketHandler, experiencesDirectory, YamlFileReader.Read);
        }
    }
}
