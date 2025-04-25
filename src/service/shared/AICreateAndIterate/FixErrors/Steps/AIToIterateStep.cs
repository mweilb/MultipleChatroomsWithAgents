using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
 
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
#pragma warning disable SKEXP0080
namespace AICreateAndIterate.FixErrors.Steps
{
    public class AIToIterateStep : KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;

        public AIToIterateStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> AIToIterateAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            string promptTemplate = $"{_initialState.PromptTemplate}\n\nOptions:\n{string.Join("\n", state.Suggestions?.Options ?? new System.Collections.Generic.List<string>())}\n\nRecommendation: {state.Recommendation?.RecommendedReason}";

            var arguments = new KernelArguments
            {
                { "options", string.Join("\n", state.Suggestions?.Options ?? new System.Collections.Generic.List<string>()) },
                { "recommendation", state.Recommendation?.RecommendedReason ?? "" }
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "0";
            int selectedIndex = 0;
            int.TryParse(result.Trim(), out selectedIndex);

            if (state.Suggestions != null)
            {
                state.Suggestions.SelectedIndex = selectedIndex;
            }

            await ctx.EmitEventAsync("AIToIterate", data: state, visibility: KernelProcessEventVisibility.Internal);

            return state;
        }
    }
}
