#pragma warning disable SKEXP0080
using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;


namespace AICreateAndIterate.FixErrors.Steps
{
    public class HumanIterateStep : KernelProcessStep<YamlFixState>
    {
        
        [KernelFunction]
        public async Task<YamlFixState> IterateWithHumanAsync(YamlFixState state, KernelProcessStepContext ctx)
        {
            await ctx.EmitEventAsync(ProcessEvents.RequestHumanInTheLoopForIterate, data: state, visibility: KernelProcessEventVisibility.Internal);
            return state;
        }
    }

}
