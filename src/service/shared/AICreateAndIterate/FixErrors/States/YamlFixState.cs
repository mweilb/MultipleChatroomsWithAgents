 
using YamlConfigurations.Validations;

namespace AICreateAndIterate.FixErrors
{



    // Main state class, composed of smaller state classes
    public record class YamlFixState
    {
        public string YamlFilePath { get; set; } = string.Empty;
        public string YamlText { get; set; } = string.Empty;
    
        public Dictionary<string, List<ValidationError>> Errors { get; set; } = [];

        public ErrorToFixRecommendation Recommendation { get; set; } = new();
        public ErrorToFixSolution? Suggestions { get; set; } = null;

        // Category/content pairs loaded from .md files
        public Dictionary<string, string> ErrorHints { get; set; } = [];

        public bool IsComplete { get; set; } = false;


        public int MaxAttempts { get; set; } = 5;
        public string? ValidationException { get; set; } = null;
        public string? NextEvent { get; set; } = null;
    }
}
