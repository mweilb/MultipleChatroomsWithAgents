using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MultiAgents.Configurations
{
    public class PresetEvaluator
    {
        public PresetMappingConfiguration MappingConfiguration { get; set; }

        public PresetEvaluator(PresetMappingConfiguration config)
        {
            MappingConfiguration = config;
        }

        /// <summary>
        /// Evaluates a single preset string against the provided value.
        /// Checks simple (single) mappings first, then falls back to colon- and equals-based mappings.
        /// </summary>
        public bool? EvaluatePreset(string preset, string value)
        {
            string trimmedPreset = preset.Trim();

            // Check for a match in the single token mappings.
            bool? singleResult = EvaluateSingleMappingPreset(trimmedPreset);
            if (singleResult.HasValue)
                return singleResult;

            // Try colon-based evaluation.
            bool? result = EvaluateColonLabelPreset(preset, value);
            if (result.HasValue)
                return result;

            // Try equals-based single label evaluation.
            result = EvaluateEqualsLabelPreset(preset, value);
            if (result.HasValue)
                return result;

            // Try equals-based multiple label evaluation.
            result = EvaluateEqualsMultipleLabelPreset(preset, value);
            if (result.HasValue)
                return result;

            return null;
        }

        /// <summary>
        /// Evaluates a list of preset strings against the provided value.
        /// Returns a tuple indicating whether any preset produced an answer and the corresponding boolean value.
        /// </summary>
        /// <param name="presets">List of preset strings.</param>
        /// <param name="value">The value to evaluate (e.g. an agent name).</param>
        /// <returns>(presetFound, answer)</returns>
        public (bool presetFound, bool answer) EvaluatePresets(List<string> presets, string value)
        {
            if (presets == null || presets.Count == 0)
                return (false, false);

            foreach (var preset in presets)
            {
                bool? result = EvaluatePreset(preset, value);
                if (result.HasValue)
                    return (true, result.Value);
            }
            return (false, false);
        }

        private bool? EvaluateSingleMappingPreset(string preset)
        {
            var mapping = MappingConfiguration.Singles.FirstOrDefault(m =>
                string.Equals(m.Label, preset, StringComparison.OrdinalIgnoreCase));
            return mapping != null ? mapping.Value : (bool?)null;
        }

        private bool? EvaluateColonLabelPreset(string preset, string value)
        {
            var pattern = $@"^(?:(?<not>{Regex.Escape(MappingConfiguration.NotToken)}\s+))?{Regex.Escape(MappingConfiguration.ColonLabelToken)}:\s*\{{(?<label>[^{{}}]+)\}}$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(preset);
            if (match.Success)
            {
                string prefix = match.Groups["not"].Value.Trim();
                string label = match.Groups["label"].Value.Trim();
                if (string.Equals(label, value, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryEvaluateMapping(prefix, label, out bool result))
                        return result;
                }
            }
            return null;
        }

        private bool? EvaluateEqualsLabelPreset(string preset, string value)
        {
            var pattern = $@"^(?:(?<not>{Regex.Escape(MappingConfiguration.NotToken)}\s+))?{Regex.Escape(MappingConfiguration.EqualsLabelToken)}\s*=\s*(?<label>[^=\[\]]+)$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(preset);
            if (match.Success)
            {
                string prefix = match.Groups["not"].Value.Trim();
                string label = match.Groups["label"].Value.Trim();
                if (string.Equals(label, value, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryEvaluateMapping(prefix, label, out bool result))
                        return result;
                }
            }
            return null;
        }

        private bool? EvaluateEqualsMultipleLabelPreset(string preset, string value)
        {
            var pattern = $@"^(?:(?<not>{Regex.Escape(MappingConfiguration.NotToken)}\s+))?{Regex.Escape(MappingConfiguration.EqualsMultipleLabelToken)}\s*=\s*\[(?<labels>[^\[\]]+(?:,[^\[\]]+)*)\]$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(preset);
            if (match.Success)
            {
                string prefix = match.Groups["not"].Value.Trim();
                string labelsGroup = match.Groups["labels"].Value;
                var labels = labelsGroup.Split(',')
                                        .Select(x => x.Trim())
                                        .Where(x => !string.IsNullOrEmpty(x));
                foreach (var label in labels)
                {
                    if (string.Equals(label, value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (TryEvaluateMapping(prefix, label, out bool result))
                            return result;
                    }
                }
            }
            return null;
        }

        private bool TryEvaluateMapping(string prefix, string label, out bool result)
        {
            result = false;
            var mapping = MappingConfiguration.Mappings.FirstOrDefault(m =>
                string.Equals(m.Prefix, prefix, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(m.Label, label, StringComparison.OrdinalIgnoreCase));
            if (mapping != null)
            {
                result = mapping.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Validates the format of a list of preset strings.
        /// Returns a list of error messages for presets that do not follow any recognized format.
        /// This method does not require a value to compare against; it only checks the structure.
        /// </summary>
        public List<string> ValidatePresetFormats(List<string> presets)
        {
            var errors = new List<string>();
            if (presets == null || presets.Count == 0)
                return errors;

            foreach (var preset in presets)
            {
                if (!IsValidPresetFormat(preset))
                {
                    errors.Add($"Invalid preset format: '{preset}'.");
                }
            }
            return errors;
        }

        /// <summary>
        /// Checks if the given preset string follows one of the recognized formats.
        /// </summary>
        private bool IsValidPresetFormat(string preset)
        {
            string trimmed = preset.Trim();

            // Check if it exactly matches a simple single mapping.
            if (MappingConfiguration.Singles.Any(m => string.Equals(m.Label, trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Define patterns based on defaults from the mapping configuration.
            string colonPattern = $@"^(?:(?<not>{Regex.Escape(MappingConfiguration.NotToken)}\s+))?{Regex.Escape(MappingConfiguration.ColonLabelToken)}:\s*\{{(?<label>[^{{}}]+)\}}$";
            string equalsSinglePattern = $@"^(?:(?<not>{Regex.Escape(MappingConfiguration.NotToken)}\s+))?{Regex.Escape(MappingConfiguration.EqualsLabelToken)}\s*=\s*(?<label>[^=\[\]]+)$";
            string equalsMultiplePattern = $@"^(?:(?<not>{Regex.Escape(MappingConfiguration.NotToken)}\s+))?{Regex.Escape(MappingConfiguration.EqualsMultipleLabelToken)}\s*=\s*\[(?<labels>[^\[\]]+(?:,[^\[\]]+)*)\]$";

            if (Regex.IsMatch(trimmed, colonPattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
            if (Regex.IsMatch(trimmed, equalsSinglePattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
            if (Regex.IsMatch(trimmed, equalsMultiplePattern, RegexOptions.IgnoreCase))
            {
                return true;
            }
            return false;
        }

    }
}
