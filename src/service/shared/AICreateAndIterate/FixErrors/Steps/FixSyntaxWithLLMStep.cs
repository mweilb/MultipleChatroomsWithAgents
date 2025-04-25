
using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
 
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using YamlConfigurations.Validations;

#pragma warning disable SKEXP0080
namespace AICreateAndIterate.FixErrors.Steps
{
    public class FixSyntaxWithLLMStep : KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;

        public FixSyntaxWithLLMStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> FixSyntaxWithLLMAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            // Use prompt from _initialState
            string promptTemplate = $"{_initialState.PromptTemplate}\n\n{state.YamlText}";

            // Check if the error keyword is available in ErrorHints
            string hints = "No hints provided.";
            if (state.ErrorHints.TryGetValue(ValidationErrorKeywords.Syntax, out var foundHint) && !string.IsNullOrWhiteSpace(foundHint))
            {
                hints = foundHint;
            }

            // Aggregate all syntax errors into a string
            var syntaxErrors = state.Errors
                .SelectMany(kvp => kvp.Value)
                .Where(e => e.Keyword == ValidationErrorKeywords.Syntax)
                .Select(e => $"Line {e.LineNumber}, Col {e.CharPosition}: {e.Message}")
                .ToList();
            string errors = string.Join("\n", syntaxErrors);

            var arguments = new KernelArguments
            {
                { "yaml", state.YamlText },
                { "errors",  errors},
                { "hints",  hints}
            };
          

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "";

            string fixedYaml = result.Trim();

            // Clean code block markers if present
            fixedYaml = CodeBlockCleaner.CleanCodeBlock(fixedYaml);

            // Fill Recommendation and Suggestions for downstream steps
            state.Recommendation = new ErrorToFixRecommendation
            {
                RecommendedIndex = 0,
                RecommendedReason = "Applied syntax fix using LLM.",
                Error = state.Errors
                    .SelectMany(kvp => kvp.Value)
                    .FirstOrDefault(e => e.Keyword == ValidationErrorKeywords.Syntax)
            };

            state.Suggestions = new ErrorToFixSolution(
                ["Fix with LLM"],
                "FixSyntaxWithLLM"
            )
            {
                SelectedIndex = 0,
                FixedYaml = fixedYaml,
                Attempts = 1,
                ExplainDifferences = "LLM attempted to fix YAML syntax errors."
            };

            // Optionally, re-validate or escalate if the fix failed
            await ctx.EmitEventAsync(ProcessEvents.RequestHumanInTheLoopForReview, data: state, visibility: KernelProcessEventVisibility.Internal);

            return state;
        }
    }
}
