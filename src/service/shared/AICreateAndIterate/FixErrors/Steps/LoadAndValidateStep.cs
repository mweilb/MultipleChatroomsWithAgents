using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;
using YamlConfigurations.FileReader;


#pragma warning disable SKEXP0080

namespace AICreateAndIterate.FixErrors.Steps
{
    public class LoadAndValidateStep : KernelProcessStep<YamlFixState>
    {

        [KernelFunction]
        public async Task<YamlFixState> ValidateYamlAsync(KernelProcessStepContext ctx, YamlFixState state)
        {
            var (syntaxValid,yamlText, experienceDict) = YamlFileReader.ReadFile(state.YamlFilePath);

            // Build validation errors dictionary
            var validationErrors = experienceDict.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors ?? new List<YamlConfigurations.Validations.ValidationError>()
            );

            bool noErrors = validationErrors.Values.All(list => list == null || list.Count == 0);
            state.Errors = validationErrors;
            state.YamlText = yamlText;
            state.IsComplete = noErrors;


            // Emit events based on validation result
            if (noErrors)
            {
                await ctx.EmitEventAsync(ProcessEvents.NoErrorsFound, data: state, visibility: KernelProcessEventVisibility.Public);  
            }
            else
            {
               await ctx.EmitEventAsync(ProcessEvents.FixAnError, data: state, visibility: KernelProcessEventVisibility.Internal);  
            }

            return state;
        }
    }

}
