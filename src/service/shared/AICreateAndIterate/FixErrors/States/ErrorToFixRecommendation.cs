
using YamlConfigurations.Validations;

namespace AICreateAndIterate.FixErrors
{
    // Holds recommendation info for the first fix step
    public record class ErrorToFixRecommendation
    {
        public int? RecommendedIndex { get; set; } = null;
        public string? RecommendedReason { get; set; } = null;
        public ValidationError? Error { get; set; } = null;
    }
}