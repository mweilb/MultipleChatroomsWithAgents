 
using YamlDotNet.Serialization;

namespace YamlConfigurations
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
 
    }

}
