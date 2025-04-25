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
 
            if (state.Suggestions == null)
                return state;

            var index = state.Suggestions.SelectedIndex;
            if (index < 0 || index >= state.Suggestions.Options.Count)
                return state;   

            var errorContext = state.Recommendation?.Error;
            if (errorContext == null)
                return state; // No errors to fix


            string suggestedFix = state.Suggestions.Options[index];

            string promptTemplate = _initialState.PromptTemplate;

            var arguments = new KernelArguments
            {
            
                {"error",errorContext.Message},
                {"suggestion",suggestedFix},
                { "yaml", state.YamlText ?? "" },
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
