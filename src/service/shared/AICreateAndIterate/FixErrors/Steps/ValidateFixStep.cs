// ValidateFixStep.cs
using Microsoft.SemanticKernel;
using YamlConfigurations.FileReader;
using AICreateAndIterate.FixErrors.Events;

#pragma warning disable SKEXP0080
namespace AICreateAndIterate.FixErrors.Steps
{
    public enum FixType
    {
        Syntax,
        Content
    }

    public record class ValidateFixState {
        public FixType fixType = FixType.Content;
    }


    public class ValidateFixStep : KernelProcessStep<ValidateFixState>
    {
        private FixType _fixType = FixType.Content;

        public override ValueTask ActivateAsync(KernelProcessStepState<ValidateFixState> state)
        {
            // Expect state.State to have a property or field indicating FixType
            // Default to Content if not present
            _fixType = state.State?.fixType ?? FixType.Content;
            return base.ActivateAsync(state);
        }

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
            int attempts = state.Suggestions.Attempts;
            bool errorStillPresent = false;
            Exception? loadException = null;

            try
            {
                var (syntaxValid, newInfo) = YamlFileReader.ReadFromString(state.Suggestions.FixedYaml);

                if (newInfo.Count == 0 || newInfo.First().Value == null)
                {
                    // If the new info is empty or null, we consider the error still present
                    errorStillPresent = true;
                }   
                else
                {
        
                    if (_fixType == FixType.Syntax)
                    {
                        errorStillPresent = !syntaxValid;
                    }
                    else
                    {     
                        // Check if the original error is still present
                        var errorContext = state.Recommendation?.Error;
                        if (errorContext != null)
                        {
                            errorStillPresent = false;
                            foreach (var room in newInfo.Values)
                            {
                                foreach (var err in room.Errors)
                                {
                                    // Set a breakpoint here to debug error matching
                                    if (err.Message == errorContext?.Message &&
                                        err.Location == errorContext?.Location)
                                    {
                                        errorStillPresent = true;
                                        break;
                                    }
                                }
                                if (errorStillPresent)
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loadException = ex;
                errorStillPresent = true;
            }

            if (state.Suggestions != null)
            {
                state.Suggestions.Attempts = attempts + (errorStillPresent && attempts < maxAttempts ? 1 : 0);
            }

            if (errorStillPresent && attempts + 1 >= maxAttempts)
            {
                state.IsComplete = false;
                await ctx.EmitEventAsync(ProcessEvents.RequestHumanInTheLoopForFailure, data: state, visibility: KernelProcessEventVisibility.Public);
            }
            else if (errorStillPresent)
            {
                state.IsComplete = true;
                if (_fixType == FixType.Syntax)
                {
                    await ctx.EmitEventAsync(ProcessEvents.TryToApplyFixAgainSyntax, data: state, visibility: KernelProcessEventVisibility.Public);
                }
                else
                {
                    await ctx.EmitEventAsync(ProcessEvents.TryToApplyFixAgain, data: state, visibility: KernelProcessEventVisibility.Public);
                }
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
