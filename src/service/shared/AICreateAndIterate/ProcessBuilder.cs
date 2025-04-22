using AICreateAndIterate.FixErrors;
using AICreateAndIterate.FixErrors.Steps;
using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0080

namespace AICreateAndIterate
{
    public class Installer
    {
        public static ProcessBuilder GetYamlErrorCheckerBuilder(YamlErrorCheckerConfig config)
        {
            var builder = new ProcessBuilder("Fix Errors");

            // --- Step Declarations ---
            var loadAndValidateStep = builder.AddStepFromType<LoadAndValidateStep>();

            var recommendStep = builder.AddStepFromType<RecommendFirstFixStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.RecommendFirstErrorPromptTemplate }
            );

            var suggestFixForErrorsStep = builder.AddStepFromType<SuggestFixForErrorsStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.SuggestFixPromptTemplate }
            );

            var iterateStep = builder.AddStepFromType<HumanIterateStep>();

            var applyStep = builder.AddStepFromType<ApplyFixStep>();

            var validateFixStep = builder.AddStepFromType<ValidateFixStep>();

            var saveFixStep = builder.AddStepFromType<SaveFixStep>();

            var humanReviewStep = builder.AddStepFromType<HumanReviewStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.HumanReviewPromptTemplate }
            );

            var eventChannelStep = builder.AddProxyStep(ProcessEvents.HumanInTheLoopEvents);
             
            // --- Event Routing (Process Flow) ---

            // Entry point
            builder.OnInputEvent(ProcessEvents.Start)
                .SendEventTo(new(loadAndValidateStep));

            builder.OnEvent(ProcessEvents.Start)
                .SendEventTo(new(loadAndValidateStep));    

            // Main process flow
            loadAndValidateStep.OnEvent(ProcessEvents.NoErrorsFound)
                .EmitExternalEvent(eventChannelStep, ProcessEvents.WaitingOnHumanFinished);

            loadAndValidateStep.OnEvent(ProcessEvents.FixAnError)
                .SendEventTo(new(recommendStep));

        
            recommendStep.OnFunctionResult()
                .SendEventTo(new(suggestFixForErrorsStep));
    

            suggestFixForErrorsStep.OnFunctionResult()
                .SendEventTo(new(iterateStep));

            // Human-in-the-loop event routing
            iterateStep.OnEvent(ProcessEvents.RequestHumanInTheLoopForIterate)
                .EmitExternalEvent(eventChannelStep, ProcessEvents.WaitingOnHumanIterate);

            validateFixStep.OnEvent(ProcessEvents.RequestHumanInTheLoopForFailure)
                .EmitExternalEvent(eventChannelStep, ProcessEvents.WaitingOnHumanValidateMaxAttempts);

            validateFixStep.OnEvent(ProcessEvents.TryToApplyFixAgain)
                .SendEventTo(new(applyStep));

            validateFixStep.OnEvent(ProcessEvents.RequestReview)
                .SendEventTo(new(humanReviewStep));

            humanReviewStep.OnEvent(ProcessEvents.RequestHumanInTheLoopForReview)
                .EmitExternalEvent(eventChannelStep, ProcessEvents.WaitingOnHumanReview);

            applyStep.OnFunctionResult()
                .SendEventTo(new(validateFixStep));

            validateFixStep.OnFunctionResult()
                .SendEventTo(new(humanReviewStep));

            // External apply fix entry
            builder.OnInputEvent(ProcessEvents.ApplyFix)
                .SendEventTo(new(applyStep));

            // External save fix entry
            builder.OnInputEvent(ProcessEvents.SaveFix)
                .SendEventTo(new(saveFixStep));

            saveFixStep.OnEvent(ProcessEvents.RequestHumanToSaveFile)
                .EmitExternalEvent(eventChannelStep, ProcessEvents.WaitingOnHumanSaveFile);

            return builder;
        }
    }
}
