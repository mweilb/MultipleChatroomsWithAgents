using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion; 
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
#pragma warning disable SKEXP0080
namespace AICreateAndIterate.FixErrors.Steps
{
    public class AIToReviewStep : KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;

        public AIToReviewStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> AIToReviewAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            string promptTemplate = $"{_initialState.PromptTemplate}\n\nFixedYaml:\n{state.Suggestions?.FixedYaml}\n\nExplainDifferences:\n{state.Suggestions?.ExplainDifferences}";

            var arguments = new KernelArguments
            {
                { "fixedYaml", state.Suggestions?.FixedYaml ?? "" },
                { "explainDifferences", state.Suggestions?.ExplainDifferences ?? "" }
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "yes";
            bool accept = result.Trim().ToLower().StartsWith("y");

            if (accept)
            {
                await ctx.EmitEventAsync(ProcessEvents.SaveFix, data: state, visibility: KernelProcessEventVisibility.Internal);
            }
            else {
                await ctx.EmitEventAsync(ProcessEvents.Start, data: state, visibility: KernelProcessEventVisibility.Internal);
            }
            return state;
        }
    }
}
