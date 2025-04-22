using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
 

#pragma warning disable SKEXP0080

namespace AICreateAndIterate.FixErrors.Steps
{
    public class SuggestFixForErrorsStep : KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;
        private const int OptionThreshold = 5;

        public SuggestFixForErrorsStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> SuggestFixesForErrorsAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            var results = new List<ErrorToFixSolution>();

            var errorContext = state.Recommendation.Error;
            if (errorContext == null)
            {
                return state; // No errors to fix
            }

            var promptTemplate = _initialState.PromptTemplate;

            var arguments = new KernelArguments
            {
                { "line", errorContext.LineNumber },
                { "col", errorContext.CharPosition },
                { "yaml", state.YamlText },
                { "error", errorContext.Message }
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "";

            var suggestions = new List<string>();
            if (!string.IsNullOrWhiteSpace(result))
            {
                var cleaned = PromptJsonCleaner.CleanJsonBlock(result);

                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(cleaned);
                    if (doc.RootElement.TryGetProperty("suggestions", out var suggestionsElement) && suggestionsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        suggestions = suggestionsElement.EnumerateArray().Select(e => e.GetString() ?? "").ToList();
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    suggestions = cleaned.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();
                }
            }

            string eventName = suggestions.Count switch
            {
                0 => ProcessEvents.AutoFixReady,
                1 => ProcessEvents.ApplyFix,
                <= OptionThreshold => ProcessEvents.ApplyFix,
                _ => ProcessEvents.IterateRequired
            };

            state.Suggestions = new ErrorToFixSolution(suggestions, eventName);
            return state;
        }
    }
}
