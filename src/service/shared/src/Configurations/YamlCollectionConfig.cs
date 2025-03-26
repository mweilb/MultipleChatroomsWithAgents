
using api.src.SemanticKernel.VectorStore;
using Microsoft.SemanticKernel;
using MultiAgents.SemanticKernel.VectorStore;
using YamlDotNet.Serialization;

namespace MultiAgents.Configurations
{

    public class YamlCollectionConfig
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        [YamlMember(Alias = "structure")]
        public string Structure { get; set; } = string.Empty;

        [YamlMember(Alias = "top")]
        public int Top { get; set; }

        [YamlMember(Alias = "skip")]
        public int Skip { get; set; }

        internal void Setup(Kernel kernel, int embeddedSize)
        {
  
            string? structureName = Structure;
            if (structureName != null && structureName == "TextParagraph")
            {
                if (embeddedSize == 3584)
                {
                    VectorStoreHelper<TextParagraphEmbeddingOf3584>.AddTextSearchPlugin(kernel, this);
                }
                else if (embeddedSize == 1536)
                {
                    VectorStoreHelper<TextParagraphEmbeddingOf1536>.AddTextSearchPlugin(kernel, this);
                }
            }
        }
    }

}
