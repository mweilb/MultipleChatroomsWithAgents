// ValidateFixStep.cs
using Microsoft.SemanticKernel;
using YamlConfigurations.FileReader;
using AICreateAndIterate.FixErrors.Steps;
using AICreateAndIterate.FixErrors.Events;

#pragma warning disable SKEXP0080
namespace AICreateAndIterate.FixErrors.Steps
{
    public class ValidateFixStep : KernelProcessStep<YamlFixState>
    {
        [KernelFunction]
        public async Task<YamlFixState> ValidateFixAsync(
            KernelProcessStepContext ctx,
            Kernel kernel,
            YamlFixState state)
        {
            if (state.Suggestions == null)
            {
                return state;
            }

            int maxAttempts = state.MaxAttempts;
            int attempts = state.Suggestions?.Attempts ?? 0;
            bool errorStillPresent = false;
            Exception? loadException = null;

            try
            {
                var newInfo = YamlFileReader.ReadFromString(state.YamlText);

                // Check if the original error is still present
                var errorContext = state.Recommendation?.Error;
                if (errorContext != null)
                {
                    errorStillPresent = newInfo.Values
                        .SelectMany(room => room.Errors)
                        .Any(err =>
                            err.Message == errorContext?.Message &&
                            err.Location == errorContext?.Location
                        );
                }
            }
            catch (Exception ex)
            {
                loadException = ex;
                errorStillPresent = true;
            }

            if (state.Suggestions != null)
            {
                state.Suggestions.FixedYaml = state.YamlText;
                state.Suggestions.Attempts = attempts + (errorStillPresent && attempts < maxAttempts ? 1 : 0);
            }

            if (errorStillPresent && attempts + 1 >= maxAttempts)
            {
                state.IsComplete = true;
                await ctx.EmitEventAsync(ProcessEvents.TryToApplyFixAgain, data: state, visibility: KernelProcessEventVisibility.Public);
            }
            else if (errorStillPresent)
            {
                state.IsComplete = false;
                await ctx.EmitEventAsync(ProcessEvents.RequestHumanInTheLoopForFailure, data: state, visibility: KernelProcessEventVisibility.Public);
            }
            else
            {
                if (state.Suggestions != null)
                    state.Suggestions.Attempts = 0;
                state.IsComplete = true;
                await ctx.EmitEventAsync(ProcessEvents.RequestReview, data: state, visibility: KernelProcessEventVisibility.Public);
            }

            state.ValidationException = loadException?.Message;
            return state;
        }
    }
}
