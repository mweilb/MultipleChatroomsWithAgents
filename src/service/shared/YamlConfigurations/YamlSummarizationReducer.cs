﻿using YamlDotNet.Serialization;

namespace YamlConfigurations
{
    public class YamlSummarizationReducer
    {
        [YamlMember(Alias = "target-count")]
        public string? TargetCount { get; set; }

        [YamlMember(Alias = "threshold-count")]
        public string? ThresholdCount { get; set; }
    }
}