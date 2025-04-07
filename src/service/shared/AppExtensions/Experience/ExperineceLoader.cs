using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using YamlConfigurations;
using YamlConfigurations.FileReader;
using Microsoft.Extensions.DependencyInjection;
using YamlConfigurations.Librarians;
using SemanticKernelExtension.Orchestrator;

namespace AppExtensions.Experience
{
    public static class ExperienceLoader
    {
        /// <summary>
        /// Reads all .yml or .yaml files under a given directory.
        /// Returns a dictionary of experience name -> YamlMultipleChatRooms object.
        /// </summary>
        /// <param name="directory">The folder path to read the YAML files from.</param>
        /// <returns>A dictionary of experience name and the corresponding YAML definitions.</returns>
        public static Dictionary<string, YamlMultipleChatRooms> LoadExperiences(string directory)
        {
            var experiences = new Dictionary<string, YamlMultipleChatRooms>();

            // Get all YAML files (.yml and .yaml) in the specified directory.
            var yamlFiles = Directory.GetFiles(directory, "*.yml")
                                     .Union(Directory.GetFiles(directory, "*.yaml"));

            // For each file, read and parse to get the YamlMultipleChatRooms objects
            foreach (var yamlFilePath in yamlFiles)
            {
                // This method presumably returns a Dictionary<string, YamlMultipleChatRooms>
                // Key: experience name, Value: the YamlMultipleChatRooms definition
                var experienceDict = YamlFileReader.Read(yamlFilePath);

                // Merge into the final dictionary
                foreach (var kvp in experienceDict)
                {
                    // If key already exists, you can decide if you want to overwrite or skip
                    if (!experiences.ContainsKey(kvp.Key))
                    {
                        experiences.Add(kvp.Key, kvp.Value);
                    }
                }
            }

            return experiences;
        }


      

        public async static Task<YamLibrarians?> GatherLibrariansAsync(YamlMultipleChatRooms config, Kernel kernel)
        {
            string RoomName = config.Name;
            string RoomEmoji = config.Emoji;

            List<YamlInstanceOfAgentConfig> activeLibrarians = [];
            List<YamlInstanceOfAgentConfig> notActiveLibrarians = [];

            var vectorStore = kernel.Services.GetService<IVectorStore>();

#pragma warning disable SKEXP0001
            var textEmbeddingGeneration = kernel.Services.GetService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001

            // Exit early if there are no rooms.
            if (config.Rooms == null)
            {
                return null;
            }

            // Get all valid collection names from the vector store at once.
            var validCollections = new HashSet<string>();
            if (vectorStore != null)
            {
                try
                {
                    await foreach (var collectionName in vectorStore.ListCollectionNamesAsync())
                    {
                        validCollections.Add(collectionName);
                    }
                }
                //could fail if not available, we handle that
                catch (Exception error)
                {
                    Console.WriteLine(error.Message.ToString());
                }
            }

            // Both services must be available.
            bool servicesAvailable = vectorStore != null && textEmbeddingGeneration != null;

            // Iterate through all rooms and their agents.
            foreach (var room in config.Rooms.Values)
            {
                foreach (var agent in room.Agents)
                {
                    if (agent.Collection == null)
                    {
                        continue;
                    }

                    // Check if the collection is valid for the specified embedding size.
                    if (validCollections.Contains(agent.Collection.Name))
                    {
                        activeLibrarians.Add(agent);
                        continue;
                    }


                    notActiveLibrarians.Add(agent);
                }
            }

            if (activeLibrarians.Count == 0 && notActiveLibrarians.Count == 0)
            {
                return null;
            }

            var yamLibrarians = new YamLibrarians()
            {
                ActiveLibrarians = activeLibrarians,
                NotActiveLibrarians = notActiveLibrarians,
                RoomEmoji = config.Emoji,
                RoomName = config.Name,
            };


            return yamLibrarians;
        }
 
    }
}
