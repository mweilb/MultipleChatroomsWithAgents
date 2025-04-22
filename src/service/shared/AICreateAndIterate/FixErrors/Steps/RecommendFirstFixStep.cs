using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
#pragma warning disable SKEXP0080

namespace AICreateAndIterate.FixErrors.Steps
{

    public class RecommendFirstFixStep : KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;

        public RecommendFirstFixStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> RecommendFirstFixAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            // Prepare a summary of all errors for the prompt
            var errorContexts = state.Errors
                .SelectMany(kvp => (kvp.Value ?? [])
                    .Select(error =>  error))
                .ToList();
            var errorSummaries = errorContexts.Select((e, i) =>
                $"[{i}] Line {e.LineNumber}, Col {e.CharPosition}: {e.Message}").ToList();

            var errorSummary = string.Join("\n", errorSummaries);

            var promptTemplate = _initialState.PromptTemplate;
            var arguments = new KernelArguments
            {
                { "yaml", state.YamlText },
                { "errors",  errorSummary}
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "";

            int index = 0;
            string reason = "No reasoning provided.";
            if (!string.IsNullOrWhiteSpace(result))
            {
                var cleaned = PromptJsonCleaner.CleanJsonBlock(result);

                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(cleaned);
                    if (doc.RootElement.TryGetProperty("index", out var idxElem) && idxElem.TryGetInt32(out var idx))
                    {
                        index = idx;
                    }
                    if (doc.RootElement.TryGetProperty("reason", out var reasonElem) && reasonElem.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        reason = reasonElem.GetString() ?? reason;
                    }
                }
                catch (System.Text.Json.JsonException)
                {
                    // fallback: try to parse index and reason from plain text
                }
            }

            // Add recommendation info to YamlFixState
            state.Recommendation.RecommendedIndex = index;
            state.Recommendation.RecommendedReason = reason;
            state.Recommendation.Error = index >= 0 && index < errorContexts.Count ? errorContexts[index] : null;

           

            return state;
        }
    }

    // RecommendationResult class is no longer needed.
}
