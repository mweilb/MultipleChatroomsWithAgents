using System.Collections.Generic;

namespace YamlConfigurations.Presets
{
    /// <summary>
    /// Represents a mapping entry.
    /// For complex mappings, both Prefix and Label are used.
    /// For simple single-token mappings, only Label is used.
    /// </summary>
    public class PresetMappingEntry
    {
        /// <summary>
        /// For complex mappings, this is the prefix (e.g. "Not")—can be empty for no prefix.
        /// </summary>
        public string Prefix { get; set; } = "";

        /// <summary>
        /// The label to match (e.g. "BotName" or a simple token like "yes").
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// The boolean value associated with this mapping.
        /// </summary>
        public bool Value { get; set; }
    }

    /// <summary>
    /// Holds the configuration for preset mappings.
    /// Contains both complex mappings (with prefix/label pairs) and simple single-token mappings.
    /// Also provides default tokens used in evaluation.
    /// </summary>
    public class PresetMappingConfiguration
    {
        /// <summary>
        /// Complex mappings for colon- or equals-based presets.
        /// </summary>
        public List<PresetMappingEntry> Mappings { get; set; } = new List<PresetMappingEntry>();

        /// <summary>
        /// Simple single-token mappings (e.g. "yes" → true, "no" → false).
        /// </summary>
        public List<PresetMappingEntry> Singles { get; set; } = new List<PresetMappingEntry>();

        /// <summary>
        /// Default token used for colon-based presets.
        /// For example: "Agent Name: {value}".
        /// </summary>
        public string ColonLabelToken { get; set; } = "Agent Name";

        /// <summary>
        /// Default token used for equals-based single value presets.
        /// For example: "AgentName = value".
        /// </summary>
        public string EqualsLabelToken { get; set; } = "AgentName";

        /// <summary>
        /// Default token used for equals-based multiple value presets.
        /// For example: "AgentNames = [value1, value2]".
        /// </summary>
        public string EqualsMultipleLabelToken { get; set; } = "AgentNames";

        /// <summary>
        /// Token indicating negation in the preset (e.g. "Not").
        /// </summary>
        public string NotToken { get; set; } = "Not";
    }
}
