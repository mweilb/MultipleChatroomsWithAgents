// Program.cs

using AICreateAndIterate.FixErrors;
 
using AppExtensions.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using AICreateAndIterate;
using AICreateAndIterate.FixErrors.Events;
 

namespace cli_client
{
#pragma warning disable SKEXP0080

    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: cli-client <path-to-yaml>");
                return;
            }

            string yamlPath = args[0];
            if (!File.Exists(yamlPath))
            {
                Console.WriteLine($"File not found: {yamlPath}");
                return;
            }

            // Load configuration (appsettings.json, localsettings.json, env vars)
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("localsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            IConfiguration configuration = configBuilder.Build();

            // Determine LLM setup from config
            var setupForLlmRequested = configuration.GetValue("LlmSetup", "Ollama");

            // Initialize Semantic Kernel
            Kernel kernel = KernelSetup.SetupKernel(configuration, setupForLlmRequested);

            // Determine configuration.yml path
            string? configLocation = configuration.GetValue<string>("ConfigurationLocation");
            string configYmlPath = !string.IsNullOrEmpty(configLocation)
                ? Path.Combine(configLocation, "configuration.yml")
                : "configuration.yml";

            // Load YAML configuration.yml
            YamlErrorCheckerConfig checkerConfig;
            if (!File.Exists(configYmlPath))
            {
                checkerConfig = new YamlErrorCheckerConfig(); // fallback to default if not found
            }
            else
            {
                try
                {
                    var yaml = File.ReadAllText(configYmlPath);
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

            var processBuilder = Installer.GetYamlErrorCheckerBuilder(checkerConfig);

            var kernelProcess = processBuilder.Build();

            // Set up external message channel for process events
            var messageChannel = new ConsoleKernelProcessMessageChannel();

            YamlFixState state = new YamlFixState()
            {
                YamlFilePath = yamlPath
            };

            KernelProcessEvent? currentEvent = new() { Id = "Start", Data = state };
            while (currentEvent != null)
            {
               
                //consumer the current event
                await kernelProcess.StartAsync(kernel, currentEvent, messageChannel);
                currentEvent = null;


                if (messageChannel.WaitingOnEvent)
                {
                    var localState = messageChannel.State;
                    if (localState == null || localState.Suggestions == null)
                    {
                        Console.WriteLine("No state available.");
                        break;
                    }

                    var suggestions = localState.Suggestions;
                    if (messageChannel.EventName == ProcessEvents.WaitingOnHumanIterate)
                    {
                        var recommendation = localState.Recommendation;
                        var options = suggestions.Options ?? [];

                        if (options.Count > 0)
                        {
                            
                            if (recommendation != null && recommendation.Error != null)
                            {
                                var err = recommendation.Error;
                                Console.WriteLine("Recommended Error to Fix:");
                                if (err != null)
                                {
                                    Console.WriteLine($"\tLine {err.LineNumber}, Column {err.CharPosition}: {err.Message}");
                                }
                                else
                                {
                                    Console.WriteLine("\tUnknown error type.");
                                }
                                Console.WriteLine($"\tReason: {recommendation.RecommendedReason}\n");
                            }
                            else
                            {
                                Console.WriteLine("No recommended error found to fix.");
                            }


                            Console.WriteLine("Available Fix Suggestions:");
                            for (int i = 0; i < options.Count; i++)
                            {
                                Console.WriteLine($"\t{i + 1}: {options[i]}");
                            }

                            Console.Write("Select a suggestion by number: ");
                            if (int.TryParse(Console.ReadLine(), out int selected) &&
                                selected > 0 && selected <= options.Count)
                            {
                                suggestions.SelectedIndex = selected - 1;
                                Console.WriteLine($"You selected: {options[selected - 1]}");
                            }
                            else
                            {
                                Console.WriteLine("Invalid selection. No suggestion selected.");
                            }

                            currentEvent = new() { Id = suggestions.EventName, Data = localState };
                        }
                    }
 
                    else if (messageChannel.EventName == ProcessEvents.WaitingOnHumanReview) {
                    
                        var answer = suggestions.FixedYaml;
                        var explainDifferences = suggestions.ExplainDifferences;
                        // Write a function to do a difference between the two ymls
                       
                        Console.WriteLine("New Yaml");
                        
                        Console.WriteLine(answer);

                        Console.WriteLine("Diff between the two ymls:");
                        
                        Console.WriteLine(explainDifferences);
                      
                        Console.WriteLine("Do you want to accept the fix? (y/n)");
                        var accept = Console.ReadLine() ?? "";
                        if ((accept.ToLower() == "y") || (accept.ToLower() == "yes"))
                        {
                            Console.WriteLine("You accepted the fix.");
                            currentEvent = new() { Id = ProcessEvents.SaveFix, Data = localState };
                        }
                        else
                        {
                            Console.WriteLine("You rejected the fix.");
                            currentEvent = new() { Id = ProcessEvents.RejectFix, Data = localState };
                        }

                    }
                    else if (messageChannel.EventName == ProcessEvents.WaitingOnHumanSaveFile)
                    {
                        if (!string.IsNullOrWhiteSpace(localState.YamlFilePath) && !string.IsNullOrEmpty(localState.Suggestions?.FixedYaml))
                        {
                            await File.WriteAllTextAsync(localState.YamlFilePath, localState.Suggestions?.FixedYaml);
                        }

                        state = new YamlFixState()
                        {
                            YamlFilePath = yamlPath
                        };

                        currentEvent = new() { Id = "Start", Data = state };


                    }
                    else if (messageChannel.EventName == ProcessEvents.WaitingOnHumanFinished)
                    {
                        Console.WriteLine("Process finished. Exiting...");
                    }


                    messageChannel.WaitingOnEvent = false;
                }
            }




            Console.WriteLine("Session ended.");
        }
    }
}

namespace cli_client
{
    // Kernel setup logic (adapted from API)
    static class KernelSetup
    {
        public static Kernel SetupKernel(IConfiguration configuration, string setupForLlmRequested)
        {
            var kernelBuilder = Kernel.CreateBuilder();

            // Configure LLM provider
            if (setupForLlmRequested == "Ollama")
            {
                KernelHelper.SetupOllama(kernelBuilder, configuration);
            }
            else
            {
                KernelHelper.SetupAzure(kernelBuilder, configuration);
            }

            // Configure vector DB provider
            if (setupForLlmRequested == "Qdrant")
            {
                KernelHelper.SetupQdrant(kernelBuilder, configuration);
            }
            else if (setupForLlmRequested == "Pinecone")
            {
                KernelHelper.SetupPinecone(kernelBuilder, configuration);
            }
            else if (setupForLlmRequested == "AzureSearch")
            {
                KernelHelper.SetupAzureSearch(kernelBuilder, configuration);
            }

            // Add logging to the kernel
            kernelBuilder.Services.AddLogging();

            // Create a base logger factory with a built‐in provider (like Console).
            
            // Build a logger factory that uses your custom provider.
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
              
            });

            kernelBuilder.Services.AddSingleton(loggerFactory);

            var kernel = kernelBuilder.Build();

            return kernel;
        }
    }

    
}
