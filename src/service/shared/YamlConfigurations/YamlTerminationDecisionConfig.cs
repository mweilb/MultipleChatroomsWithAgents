using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlConfigurations.Presets;
using YamlDotNet.Serialization;

namespace YamlConfigurations
{

    // The decision object used in various places.
    public class YamlTerminationDecisionConfig  
    {  
        //New code and the rest of the above will be cleaned up
        [YamlMember(Alias = "continuation-agent-name")]
        public string? ContinuationAgentName { get; set; } 

        [YamlMember(Alias = "regex-termination")]
        public YarmRegexTermination? RegexTermination { get; set; }

        [YamlMember(Alias = "constant-termination")]
        public YamlConstantTermination? ConstantTermination { get; set; }

        [YamlMember(Alias = "prompt-termination")]
        public YamlPromptTermination? PromptTermination { get; set; }
    }
 
}
