using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlConstantTermination
    {
        [YamlMember(Alias = "agents")]
        public List<string>? Agents { get; set; }

        [YamlMember(Alias = "value")]
        public string? Value { get; set; }
    }
}
