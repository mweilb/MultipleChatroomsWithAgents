using Microsoft.SemanticKernel;
using AICreateAndIterate.FixErrors.Events;
 
#pragma warning disable SKEXP0080


namespace AICreateAndIterate.FixErrors.Steps
{
    public class SaveFixStep : KernelProcessStep<YamlFixState>
    {
        [KernelFunction]
        public async Task<YamlFixState> SaveFixAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
 
            if (state.Suggestions == null)
            {
                return state;
            }

            state.Suggestions.FixedYaml = CodeBlockCleaner.RestoreHandlebarsBraces( state.Suggestions.FixedYaml);// Check if the file path is valid
            await ctx.EmitEventAsync(ProcessEvents.RequestHumanToSaveFile, data: state);

          
            return state;
        }
    }
}
