using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlJsonParser : YamlLineInfo
    {
        // Either 'format' or 'variable' will be provided.
        [YamlMember(Alias = "format")]
        public string? Format { get; set; }

        [YamlMember(Alias = "variable")]
        public string? Variable { get; set; }
    }
}