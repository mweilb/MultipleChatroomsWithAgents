
using YamlDotNet.Serialization;

namespace  YamlConfigurations
{
    // An instance of an agent in a room, which may include position overrides.
    public class YamlInstanceOfAgentConfig : YamlAgentConfig
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;


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

    }

}
