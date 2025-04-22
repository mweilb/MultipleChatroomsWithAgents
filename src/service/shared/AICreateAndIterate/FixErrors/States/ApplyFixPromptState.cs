namespace AICreateAndIterate.FixErrors.States
{
    public record class ApplyFixPromptState : YamlFixState
    {
        public string PromptTemplate { get; set; } = string.Empty;
    }
}
