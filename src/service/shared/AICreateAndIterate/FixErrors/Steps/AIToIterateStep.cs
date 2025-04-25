using AICreateAndIterate.FixErrors.Events;
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


            var errorContext = state.Recommendation?.Error;
            if (errorContext == null)
                return state; // No errors to fix

 

            var promptTemplate = _initialState.PromptTemplate;

            // Check if the error keyword is available in ErrorHints
            string hints = "No hints provided.";
            if (!string.IsNullOrWhiteSpace(errorContext.Keyword) && state.ErrorHints != null)
            {
                if (state.ErrorHints.TryGetValue(errorContext.Keyword, out var foundHint) && !string.IsNullOrWhiteSpace(foundHint))
                {
                    hints = foundHint;
                }
            }

            var arguments = new KernelArguments
            {
                {"hints",errorContext.Message},
                {"error",errorContext.Message},
                {"yaml", state.YamlText ?? "" },
                {"options", string.Join("\n", state.Suggestions?.Options?.Select((opt, index) => $"{index + 1}. {opt}") ?? new System.Collections.Generic.List<string>())},
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "0";
       
            int.TryParse(result.Trim(), out int selectedIndex);

            if (state.Suggestions != null)
            {
                state.Suggestions.SelectedIndex = selectedIndex;
            }

            await ctx.EmitEventAsync(ProcessEvents.ApplyFix, data: state, visibility: KernelProcessEventVisibility.Internal);

            return state;
        }
    }
}
