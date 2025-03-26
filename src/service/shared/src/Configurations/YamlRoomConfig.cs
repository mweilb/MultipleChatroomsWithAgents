
 
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
 
using MultiAgents.AgentsChatRoom.Rooms;
using MultiAgents.AgentsChatRoom.WebSockets;
using MultiAgents.SemanticKernel.Modifications;
using YamlDotNet.Serialization;


namespace MultiAgents.Configurations
{

    // Room definition used in "chatrooms".
    public class YamlRoomConfig : MultiAgentChatRoom, IAgentStrategies, IMultiAgentHandler
    {

        public override string GroupName { get; set; } = string.Empty;

        [YamlMember(Alias = "emoji")]
        public override string Emoji { get; set; } = string.Empty;

        [YamlMember(Alias = "name")]
        public override string Name { get; set; } = string.Empty;

        // Agents are defined as a list (to preserve order) with each reference including a name.
        [YamlMember(Alias = "agents")]
        public List<YamlInstanceOfAgentConfig> Agents { get; set; } = [];

        // Strategies for the room.
        [YamlMember(Alias = "strategies")]
        public YamlStrategyConfig? Strategies { get; set; }

        // Moderation rule.
        [YamlMember(Alias = "moderation")]
        public YamlModerationConfig? Moderation { get; set; }



        public YamlStratergyRules? GetRuleBasedOnCurrentAgent(Agent? agent,string lastTermination, bool enteringFirstTime)
        {
            if (Strategies == null || Strategies.Rules == null)
            {
                return null;
            }

            if (Strategies.Rules.Count == 1)
            {
                return Strategies.Rules.First();
            }


            //start off by looking for start if no agent.
            if (agent == null && enteringFirstTime)
            {
                var ruleStart = Strategies.Rules.FirstOrDefault(rule =>
                    rule.Current.Any(c => (c.Name == "start"))
                );

                if (ruleStart != null)
                {
                    return ruleStart;
                }
            }
            else if (agent == null)
            {
                var ruleBasedOnLastTermination = Strategies.Rules.FirstOrDefault(rule =>
                    rule.Current.Any(c => (c.Name == lastTermination)));

                if (ruleBasedOnLastTermination != null)
                {
                    return ruleBasedOnLastTermination;
                }
            }

            //now look for the others in order
            var rule = Strategies.Rules.FirstOrDefault(rule =>
                rule.Current.Any(c => c.Name == "any" || (agent == null ? (c.Name == "user") : c.Name == agent.Name))
            );

            if (rule == null)
            {
                return Strategies.Rules.First();
            }

            return rule;
        }


        internal void Setup(Kernel kernel, int embeddedSize, ILoggerFactory loggerFactory)
        {
            List<ChatCompletionAgent> completionAgents = new List<ChatCompletionAgent>();
            // Iterate over each agent entry defined in the YAML configuration.
            foreach (var agentEntry in Agents)
            {
                agentEntry.Setup(kernel, embeddedSize);
                if (agentEntry.ChatAgent != null)
                {
                    completionAgents.Add(agentEntry.ChatAgent);
                }
            }

            Strategies?.Setup(this,kernel);

            Moderation?.Setup(kernel, embeddedSize, loggerFactory);

            ILogger<IMultiAgentChatRoom> logger = loggerFactory.CreateLogger<IMultiAgentChatRoom>();

            var agentStreamingChatRoom = new AgentStreamingChatRoom(this, kernel);
            agentStreamingChatRoom.InitGroupChat(completionAgents);

            Initialize(agentStreamingChatRoom, logger);
        }

        internal override string GetEmoji(string name) => Agents.FirstOrDefault(agent => agent.Name == name)?.Emoji ?? "";

        /// <summary>
        /// Retrieves the termination and selection streaming strategies based on the last agent.
        /// </summary>
        /// <param name="lastAgent">The last selected agent.</param>
        /// <returns>A tuple containing the termination and selection streaming strategies.</returns>
        (TerminationStreamingStrategy termination, SelectionStreamingStrategy selection) IAgentStrategies.GetStreamingStrategies(Agent? agent, string lastTermination, bool EnteringFirstTime)
        {
            var rule = GetRuleBasedOnCurrentAgent(agent, lastTermination,  EnteringFirstTime)
                 ?? throw new InvalidOperationException("No rule has been defined. Please define at least one rule.");

            var terminationStrategy = rule.TerminationStreamingStrategy
                ?? throw new InvalidOperationException("No termination rule has been defined. Please define at least one termination .");

            var selectionStrategy = rule.SelectionStreamingStrategy
                ?? throw new InvalidOperationException("No selection rule has been defined. Please define at least one selection.");

            return (terminationStrategy, selectionStrategy);
        }

        //empty moderator, dervived class to implement
        public override void EngageModerator(IWebSocketSender sender, string userId, string command, string transactionId, string textToModerate)
        {
            Moderation?.EngageModerator(sender, userId, command, transactionId, textToModerate);
        }

    }
}
 