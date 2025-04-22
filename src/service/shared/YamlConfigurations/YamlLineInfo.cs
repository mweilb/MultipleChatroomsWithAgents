
using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlLineInfo
    {
        [YamlMember(Alias = "start-line")]
        public long StartLine { get; set; }

        [YamlMember(Alias = "start-column")]
        public long StartColumn { get; set; }


    }
}
