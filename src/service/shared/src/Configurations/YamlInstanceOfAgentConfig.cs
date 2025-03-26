using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using YamlDotNet.Serialization;

namespace  MultiAgents.Configurations
{
    // An instance of an agent in a room, which may include position overrides.
    public class YamlInstanceOfAgentConfig : YamlAgentConfig
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        [YamlMember(Alias = "position")]
        public YamlPositionConfig? Position { get; set; }


        public void ApplyParentOverride( YamlAgentConfig parent)
        {
            if (Emoji == string.Empty) { Emoji = parent.Emoji; }
            if (Model == string.Empty) { Model = parent.Model; }
            if (Instructions == string.Empty) { Instructions = parent.Instructions; }

            if (Collection == null && parent.Collection != null)
            {
                Collection = parent.Collection;
            }
        }

        public ChatCompletionAgent? ChatAgent = null;

        internal void Setup(Kernel kernel, int embeddedSize)
        {
            string agentName = Name;

            // Skip if the agent configuration or its instructions are missing.
            if (Instructions == null)
            {
                return;
            }


            // Create a new ChatCompletionAgent instance with the provided properties.
            ChatAgent = new ChatCompletionAgent
            {
                Name = agentName,
                Instructions = Instructions,
                Kernel = kernel
            };


            // If the agent has collection, setup the SK plugin
            if (Collection != null)
            {
                Collection.Setup(kernel, embeddedSize);
            }


        }
    }

}
