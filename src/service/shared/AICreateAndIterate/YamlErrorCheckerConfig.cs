namespace AICreateAndIterate
{
    // Extend this class as needed for future configuration options
    public class YamlErrorCheckerConfig
    {
        public string SuggestFixPromptTemplate { get; set; } = string.Empty;
        public string RecommendFirstErrorPromptTemplate { get; set; } = string.Empty;
        public string HumanReviewPromptTemplate { get; set; } = string.Empty;
        public string ApplyFixStepPrompt { get; set; } = string.Empty;
        public string FixSyntaxWithLLMPromptTemplate { get; set; } = string.Empty;
    }
}
