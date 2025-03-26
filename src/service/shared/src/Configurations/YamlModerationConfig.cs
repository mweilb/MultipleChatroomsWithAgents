using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using MultiAgents.AgentsChatRoom.WebSockets;
using MultiAgents.SemanticKernel;
using YamlDotNet.Serialization;

namespace MultiAgents.Configurations
{
    // The decision object used in various places.
    public class YamlModerationConfig
    {
        [YamlMember(Alias = "prompt")]
        public string Prompt { get; set; } = string.Empty;

        private Kernel? _kernel = null;
        internal void Setup(Kernel kernel, int embeddedSize, ILoggerFactory loggerFactory)
        {
            _kernel = kernel;
        }

        public void EngageModerator(IWebSocketSender sender, string userId, string command, string transactionId, string textToModerate)
        {
            if (_kernel == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(Prompt))
            {
                return;
            }

            if (string.IsNullOrEmpty(textToModerate))
            {
                return;
            }


            // Fire-and-forget: launch the asynchronous work in a background task.
            Task.Run(async () =>
            {
                try
                {
                    var arguments = new KernelArguments { { "messages", textToModerate } };
                    var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

                    var response = await _kernel.InvokePromptAsync(
                        this.Prompt,
                        arguments,
                        templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                        promptTemplateFactory: promptTemplateFactory);

                    if (response != null)
                    {
                        string result = response.ToString();
                        string noThinkingResult = OllamaHelper.RemoveThinkContent(result);
                        // Check if the result contains "yes" in a case-insensitive manner.
                        if (noThinkingResult.Contains("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            // "yes" was found in the result.
                            // For example, send a message or log the result.
                            await sender.SendModerationConcern(userId, command,transactionId, textToModerate, noThinkingResult);
                        }
                    }

                }
                catch (Exception ex)
                {
                    await sender.SendError(userId, command, "Moderator", ex.ToString());
                }
            });
        }
    }
}
