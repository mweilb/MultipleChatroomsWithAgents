 
using Microsoft.SemanticKernel;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AICreateAndIterate.FixErrors;
using AICreateAndIterate.FixErrors.Events;
using AICreateAndIterate.FixErrors.Steps;

#pragma warning disable SKEXP0080
namespace AICreateAndIterate
{
    public class YamlFixProcessIterator
    {
        private readonly Kernel _kernel;
        private readonly KernelProcess _kernelProcess;
        private readonly ConsoleKernelProcessMessageChannel _messageChannel;
        private readonly Dictionary<string, string> _ErrorHints = new();

        public YamlFixProcessIterator(
            Kernel kernel,
            string yamlConfigPath)
        {
            _kernel = kernel;

              // Load YAML configuration.yml
            YamlErrorCheckerConfig checkerConfig;
            checkerConfig = LoadConfigurations(yamlConfigPath);
            LoadHints(yamlConfigPath);

     
            var processBuilder = GetYamlErrorCheckerBuilder(checkerConfig);
            _kernelProcess = processBuilder.Build();
            _messageChannel = new ConsoleKernelProcessMessageChannel();
        }

        private void LoadHints(string yamlConfigPath)
        {
            // Load .md files as category/content dictionary
            try
            {
                var directory = Path.GetDirectoryName(yamlConfigPath) ?? "";
                if (Directory.Exists(directory))
                {
                    foreach (var mdFile in Directory.GetFiles(directory, "*.md"))
                    {
                        var key = Path.GetFileNameWithoutExtension(mdFile);
                        var value = File.ReadAllText(mdFile);
                        _ErrorHints[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading .md files: {ex.Message}");
            }
        }

        private static YamlErrorCheckerConfig LoadConfigurations(string yamlConfigPath)
        {
            YamlErrorCheckerConfig checkerConfig;
            if (!File.Exists(yamlConfigPath))
            {
                checkerConfig = new YamlErrorCheckerConfig(); // fallback to default if not found
            }
            else
            {
                try
                {
                    var yaml = File.ReadAllText(yamlConfigPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    checkerConfig = deserializer.Deserialize<YamlErrorCheckerConfig>(yaml) ?? new YamlErrorCheckerConfig();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading configuration.yml: {ex.Message}");
                    throw;
                }
            }

            return checkerConfig;
        }

  private static ProcessBuilder GetYamlErrorCheckerBuilder(YamlErrorCheckerConfig config)
        {
            var builder = new ProcessBuilder("Fix Errors");

            // --- Step Declarations ---
            var loadAndValidateStep = builder.AddStepFromType<LoadAndValidateStep>();
            var fixSyntaxWithLLMStep = builder.AddStepFromType<FixSyntaxWithLLMStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.FixSyntaxWithLLMPromptTemplate }
            );

            var recommendStep = builder.AddStepFromType<RecommendFirstFixStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.RecommendFirstErrorPromptTemplate }
            );

            var suggestFixForErrorsStep = builder.AddStepFromType<SuggestFixForErrorsStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.SuggestFixPromptTemplate }
            );

            var iterateStep = builder.AddStepFromType<HumanIterateStep>();
            var aiIterateStep = builder.AddStepFromType<AIToIterateStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.RecommendFirstErrorPromptTemplate }
            );

            var applyStep = builder.AddStepFromType<ApplyFixStep, InputPromptState>(
                 new InputPromptState{ PromptTemplate = config.ApplyFixStepPrompt});
            
            var validateFixStep = builder.AddStepFromType<ValidateFixStep>();

            var saveFixStep = builder.AddStepFromType<SaveFixStep>();

            var humanReviewStep = builder.AddStepFromType<HumanReviewStep, InputPromptState>(
                new InputPromptState { PromptTemplate = config.HumanReviewPromptTemplate }
            );
            var aiReviewStep = builder.AddStepFromType<AIToReviewStep, InputPromptState>(
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

            loadAndValidateStep.OnEvent(ProcessEvents.FixSyntaxWithLLM)
                .SendEventTo(new(fixSyntaxWithLLMStep));

            loadAndValidateStep.OnEvent(ProcessEvents.FixAnError)
                .SendEventTo(new(recommendStep));

            fixSyntaxWithLLMStep.OnFunctionResult()
                .SendEventTo(new(humanReviewStep));

        
            recommendStep.OnFunctionResult()
                .SendEventTo(new(suggestFixForErrorsStep));
    

            suggestFixForErrorsStep.OnFunctionResult()
                .SendEventTo(new(iterateStep));

            // Human-in-the-loop event routing
            iterateStep.OnEvent(ProcessEvents.RequestHumanInTheLoopForIterate)
                .EmitExternalEvent(eventChannelStep, ProcessEvents.WaitingOnHumanIterate);

            // AI event routing
            builder.OnInputEvent(ProcessEvents.AIToIterate)
                .SendEventTo(new(aiIterateStep));
            
            builder.OnInputEvent(ProcessEvents.AIToReview)   
                .SendEventTo(new(aiReviewStep));

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
                .EmitExternalEvent(eventChannelStep, ProcessEvents.RequestSystemSaveFile);

            return builder;
        }


        public async IAsyncEnumerable<KernelProcessEvent> IterateAsync(string yamlFileLocation)
        {
            YamlFixState state = new YamlFixState()
            {
                YamlFilePath = yamlFileLocation,
                ErrorHints = _ErrorHints
            };

            KernelProcessEvent? currentEvent = new() { Id = "Start", Data = state };
            while (currentEvent != null)
            {
                await _kernelProcess.StartAsync(_kernel, currentEvent, _messageChannel);
                currentEvent = null;

                if (_messageChannel.WaitingOnEvent)
                {
                    var localState = _messageChannel.State;
                    if (localState == null || localState.Suggestions == null)
                    {
                        yield return null!;
                        break;
                    }

                    var suggestions = localState.Suggestions;
                    if (_messageChannel.EventName == ProcessEvents.WaitingOnHumanIterate)
                    {
                        yield return new KernelProcessEvent
                        {
                            Id = ProcessEvents.WaitingOnHumanIterate,
                            Data = localState
                        };
                        currentEvent = new() { Id = suggestions.EventName, Data = localState };
                    }
                    else if (_messageChannel.EventName == ProcessEvents.WaitingOnHumanReview)
                    {
                        yield return new KernelProcessEvent
                        {
                            Id = ProcessEvents.WaitingOnHumanReview,
                            Data = localState
                        };
                    }
                    else if (_messageChannel.EventName == ProcessEvents.RequestSystemSaveFile)
                    {
                        if (!string.IsNullOrWhiteSpace(localState.YamlFilePath) && !string.IsNullOrEmpty(localState.Suggestions?.FixedYaml))
                        {
                            await File.WriteAllTextAsync(localState.YamlFilePath, localState.Suggestions?.FixedYaml);
                        }

                        state = new YamlFixState()
                        {
                            YamlFilePath = yamlFileLocation,
                            ErrorHints = _ErrorHints
                        };

                        yield return new KernelProcessEvent
                        {
                            Id = ProcessEvents.RequestSystemSaveFile,
                            Data = localState
                        };

                        currentEvent = new() { Id = "Start", Data = state };
                    }
                    else if (_messageChannel.EventName == ProcessEvents.WaitingOnHumanFinished)
                    {
                        yield return new KernelProcessEvent
                        {
                            Id = ProcessEvents.WaitingOnHumanFinished,
                            Data = localState
                        };
                        break;
                    }

                    _messageChannel.WaitingOnEvent = false;
                }
            }
        }
    }
}
