﻿using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.Net; 
#pragma warning disable SKEXP0080

namespace AICreateAndIterate.FixErrors.Steps
{
    public class ApplyFixStep:KernelProcessStep<InputPromptState>
    {
        private InputPromptState _initialState;
      

        public ApplyFixStep()
        {
            _initialState = new InputPromptState { PromptTemplate = "" };
        }


        public override ValueTask ActivateAsync(KernelProcessStepState<InputPromptState> state)
        {
            _initialState = state.State!;
            return base.ActivateAsync(state);
        }

        [KernelFunction]
        public async Task<YamlFixState> ApplyFixAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            if (state.Suggestions == null)
                return state;

            // Get error context if available
            var errorContext = state.Recommendation?.Error;
            if (errorContext == null)
                return state; // No errors to fix

            var index = state.Suggestions.SelectedIndex;
            if (index < 0 || index >= state.Suggestions.Options.Count)
                return state;       


            string hints = "No hints provided.";

            if (!string.IsNullOrWhiteSpace(errorContext.Keyword) && state.ErrorHints != null)
            {
                if (state.ErrorHints.TryGetValue(errorContext.Keyword, out var foundHint) && !string.IsNullOrWhiteSpace(foundHint))
                {
                    hints = foundHint;
                }
            }

            // Prompt template for LLM
            var arguments = new KernelArguments
            {
                { "yaml", state.YamlText },
                { "patch", state.Suggestions.Options[index] },
                { "location", errorContext.Location ??"[No Location Provide]" },
                { "line", errorContext.LineNumber },
                { "col", errorContext.CharPosition },
                { "hints", hints },
                { "error", errorContext.Message }
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                _initialState.PromptTemplate,
                arguments,
                templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                promptTemplateFactory: promptTemplateFactory
            );

            var result = response.GetValue<string>() ?? "";

            // Clean up code block markers if present
            var cleaned = result.Trim();
            if (cleaned.StartsWith("```yaml"))
                cleaned = cleaned.Substring(7).TrimStart();
            if (cleaned.StartsWith("```"))
                cleaned = cleaned.Substring(3).TrimStart();
            if (cleaned.EndsWith("```"))
                cleaned = cleaned.Substring(0, cleaned.Length - 3).TrimEnd();    

            state.Suggestions.FixedYaml = DecodeHtmlEntities(cleaned);

            return state;
        }

        public static string DecodeHtmlEntities(string htmlEncodedYaml)
        {
            if (htmlEncodedYaml == null) throw new ArgumentNullException(nameof(htmlEncodedYaml));
            return WebUtility.HtmlDecode(htmlEncodedYaml);
        }
    }

}
