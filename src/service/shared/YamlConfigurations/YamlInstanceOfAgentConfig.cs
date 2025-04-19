﻿﻿﻿﻿
using YamlDotNet.Serialization;

namespace  YamlConfigurations
{
    // An instance of an agent in a room, which may include position overrides.
    public class YamlInstanceOfAgentConfig : YamlAgentConfig
    {
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        public static bool IsValidAgentName(string name)
        {
            // Azure OpenAI: ^[^\s<|\\/>]+$
            return System.Text.RegularExpressions.Regex.IsMatch(
                name,
                @"^[^\s<|\\/>]+$"
            );
        }

        public static bool IsValidRoomName(string name)
        {
            // Same rule as agent names: no spaces or < | \ / >
            return System.Text.RegularExpressions.Regex.IsMatch(
                name,
                @"^[^\s<|\\/>]+$"
            );
        }


        public void ApplyParentOverride( YamlAgentConfig parent)
        {
            if (string.IsNullOrEmpty(Emoji)) { DisplayName = parent.DisplayName; }
            if (string.IsNullOrEmpty(Emoji)) { Emoji = parent.Emoji; }
            if (string.IsNullOrEmpty(Model)) { Model = parent.Model; }
            if (string.IsNullOrEmpty(Instructions)) { Instructions = parent.Instructions; }
            if (string.IsNullOrEmpty(Echo)) { Echo = parent.Echo; }

            if (Collection == null && parent.Collection != null)
            {
                Collection = parent.Collection;
            }
        }
 
    }

}
