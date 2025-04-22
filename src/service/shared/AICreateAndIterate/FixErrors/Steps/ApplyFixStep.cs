using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
 
#pragma warning disable SKEXP0080

namespace AICreateAndIterate.FixErrors.Steps
{
    public class ApplyFixStep : KernelProcessStep<YamlFixState>
    {
        [KernelFunction]
        public async Task<YamlFixState> ApplyFixAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            if (state.Suggestions == null)
                return state;

            var index = state.Suggestions.SelectedIndex;
            if (index < 0 || index >= state.Suggestions.Options.Count)
                return state;

            var patch = state.Suggestions.Options[index];
            var originalYaml = state.YamlText;



            // Get error context if available
            var errorContext = state.Recommendation?.Error;
            var errorMessage = errorContext?.Message ?? "";
            var errorLine = errorContext?.LineNumber ?? 0;
            var errorCol = errorContext?.CharPosition ?? 0;

            // Prompt template for LLM
            var prompt = @"You are an expert YAML fixer. Given the original YAML, a specific error to fix, and a patch/fix description, apply the patch ONLY to fix the described error. Do not change unrelated parts of the YAML.

                Original YAML:
                {{yaml}}

                Error to fix:
                Message: {{errorMessage}}
                Line: {{errorLine}}
                Column: {{errorCol}}

                Patch/Fix:
                {{patch}}

                Return ONLY the fixed YAML.";

            var arguments = new KernelArguments
            {
                { "yaml", originalYaml },
                { "patch", patch },
                { "errorMessage", errorMessage },
                { "errorLine", errorLine },
                { "errorCol", errorCol }
            };

            var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

            var response = await kernel.InvokePromptAsync(
                prompt,
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

            state.YamlText = cleaned;

            return state;
        }
    }

}
