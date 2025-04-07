namespace YamlConfigurations
{
    using System.Collections.Generic;
    using YamlDotNet.Serialization;

    public class YamlRoundRobinSelection
    {
        [YamlMember(Alias = "initial-agent")]
        public string? InitialAgent { get; set; }

        [YamlMember(Alias = "agents")]
        public List<string>? Agents { get; set; }
    }

}