using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YamlConfigurations.Presets;
using YamlDotNet.Serialization;

namespace YamlConfigurations
{

    // The decision object used in various places.
    public class YamlTerminationDecisionConfig : YamlDecisionConfig
    {
        public static PresetMappingConfiguration MappingConfiguration => new()
        {
            Mappings =
            [
                new () { Prefix = "Not", Label = "Agent Name", Value = false },
                new () { Prefix = "",    Label = "Agent Name", Value = true },
                new () { Prefix = "Not", Label = "Agent Names", Value = false },
                new () { Prefix = "",    Label = "Agent Names", Value = true }
            ],
            Singles =
            [
                new() { Label = "yes", Value = true },
                new() { Label = "no",  Value = false }
            ]
        };

        // Expose a PresetEvaluator that uses the above MappingConfiguration.
        public static PresetEvaluator Evaluator => new(MappingConfiguration);

        /// <summary>
        /// Evaluates the preset strings (inherited via the base class property "Presets")
        /// against the provided value. The evaluation is performed by the PresetEvaluator.
        /// Returns a tuple where:
        ///   - presetFound is true if at least one preset produces an answer,
        ///   - answer is the boolean result from the first preset that produces an answer.
        /// If none of the presets yield an answer, (false, false) is returned.
        /// </summary>
        /// <param name="value">The value to evaluate (e.g. an agent name like "BotName").</param>
        public (bool presetFound, bool answer) ShouldStop(string value)
        {
            return Evaluator.EvaluatePresets(Presets, value);
        }


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
