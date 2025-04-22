using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

#pragma warning disable SKEXP0080

namespace AICreateAndIterate.FixErrors.Steps
{
    public class HumanReviewStep : KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;

        public HumanReviewStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> ReviewFixAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            var suggestions = state.Suggestions;
            var errorContext = state.Recommendation?.Error;
     
            // Use LLM to explain differences between current and fixed YAML
            if ((suggestions == null) || (errorContext == null) || string.IsNullOrWhiteSpace(state.YamlText) || string.IsNullOrWhiteSpace(suggestions.FixedYaml))
            { 
                return state;
            }

            var promptTemplate = _initialState.PromptTemplate;
            
     
            var arguments = new KernelArguments
            {
                { "line", errorContext.LineNumber },
                { "col", errorContext.CharPosition },
                { "yaml", state.YamlText },
                { "error", errorContext.Message },
                { "fixedYaml",  suggestions.FixedYaml}
            };

    

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                promptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "";
            
            suggestions.ExplainDifferences = result;
                
            // Present suggestions to the user via your UI
            await ctx.EmitEventAsync(ProcessEvents.RequestHumanInTheLoopForReview, state);
            return state;
        }
    }
}
