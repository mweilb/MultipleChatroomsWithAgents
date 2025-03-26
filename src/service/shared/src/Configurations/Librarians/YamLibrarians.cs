using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using MultiAgents.SemanticKernel.VectorStore;
using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using System.Threading.Tasks;

namespace MultiAgents.Configurations.Librarians
{
    public class YamLibrarians
    {
        public List<YamlInstanceOfAgentConfig> ActiveLibrarians { get; } = [];
        public List<YamlInstanceOfAgentConfig> NotActiveLibrarians { get; } = [];
        public string RoomName { get; private set; } = string.Empty;
        public string RoomEmoji { get; private set; } = string.Empty;

        public async Task<bool> GatherLibrariansAsync(YamlMultipleChatRooms config, Kernel kernel)
        {
            RoomName = config.Name;
            RoomEmoji = config.Emoji;

            var vectorStore = kernel.Services.GetService<IVectorStore>();

#pragma warning disable SKEXP0001
            var textEmbeddingGeneration = kernel.Services.GetService<ITextEmbeddingGenerationService>();
#pragma warning restore SKEXP0001

            // Exit early if there are no rooms.
            if (config.Rooms == null)
            {
                return false;
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
                    catch(Exception error) {
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
                        ActiveLibrarians.Add(agent);
                        continue;
                    }
                   

                    NotActiveLibrarians.Add(agent);
                }
            }

            return (ActiveLibrarians.Count + NotActiveLibrarians.Count) > 0;
        }
    }
}
