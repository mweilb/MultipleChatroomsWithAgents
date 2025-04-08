using Microsoft.SemanticKernel;

namespace SemanticKernelExtension.Orchestrator
{

    public class StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes actionType, 
        string orchestratorName, string chatName, string agentName, 
        StreamingChatMessageContent? content = null)
    {
        public enum ActionTypes
        {
            Invalid = -1,
            Error,
            AgentStarted,
            AgentUpdated,
            AgentFinsihed,
            RoomChange,
            RoomMessageStarted,
            RoomMessageUpdated,
            RoomMessageFinished,
        }

        /// <summary>
        /// The actual content (if any) from the streaming process.
        /// </summary>
        public StreamingChatMessageContent? Content { get; set; } = content;

        /// <summary>
        /// What "event" is happening: Start, Update, End, Error, etc.
        /// </summary>
        public ActionTypes Action { get; set; } = actionType;

        /// <summary>
        /// Name or identifier of the agent, if useful for the consumer.
        /// </summary>
        public string AgentName { get; set; } = agentName;

        /// <summary>
        /// Optional: orchestrator name or group name, if needed.
        /// </summary>
        public string OrchestratorName { get; set; } = orchestratorName;

        /// <summary>
        /// Optional: chat name or conversation name, if needed.
        /// </summary>
        public string ChatName { get; set; } = chatName;

    }
}
