using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace YamlConfigurations
{


    public class YamlPromptTermination
    {
        [YamlMember(Alias = "agents")]
        public List<string>? Agents { get; set; }

        [YamlMember(Alias = "instructions")]
        public string? Instructions { get; set; }

        [YamlMember(Alias = "history-variable-name")]
        public string? HistoryVariableName { get; set; }

        [YamlMember(Alias = "result-parser")]
        public YamlResultParser? ResultParser { get; set; }

        [YamlMember(Alias = "evaluate-name-only")]
        public string? EvaluateNameOnly { get; set; }


        [YamlMember(Alias = "truncation-reducer")]
        public YamlTruncationReducer? TruncationReducer { get; set; }

        [YamlMember(Alias = "summarization-reducer")]
        public YamlSummarizationReducer? SummarizationReducer { get; set; }
    }

}
