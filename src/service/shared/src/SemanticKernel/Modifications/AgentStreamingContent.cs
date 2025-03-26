using System.Text;
using Microsoft.SemanticKernel;

namespace MultiAgents.SemanticKernel.Modifications
{
    /// <summary>
    /// Represents the streaming content of an agent's message in a multi-agent chat.
    /// Inherits from <see cref="StreamingChatMessageContent"/> and adds additional properties
    /// such as hints, agent name, and flags for new agent, message completion, and chat completion.
    /// </summary>
    public class AgentStreamingContent : StreamingChatMessageContent
    {
        /// <summary>
        /// Gets the dictionary of hints containing metadata for the streaming message.
        /// </summary>
        public Dictionary<string, object> Hints { get; }

        /// <summary>
        /// Gets or sets the name of the agent sending the message.
        /// </summary>
        public string? AgentName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the first message from a new agent.
        /// </summary>
        public bool IsNewAgent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current message is complete.
        /// </summary>
        public bool IsMessageDone { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entire chat conversation is complete.
        /// </summary>
        public bool IsChatComplete { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this system is requesting an agent room change
        /// </summary>
        public bool RequestChatRoomChange { get; set; }
        
        /// <summary>
        /// Gets or sets the requested chat room name
        /// </summary>
        public string RequestedChatRoom { get; internal set; }

        /// <summary>
        /// Gets or sets the requested chat room content
        /// </summary>
        public string RequestedChatRoomContext { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentStreamingContent"/> class with default values.
        /// </summary>
        public AgentStreamingContent()
            // Base constructor is called with default values:
            // role: null, content: null, innerContent: null, choiceIndex: 0, modelId: null, encoding: UTF8, metadata: null.
            : base(null, null, null, 0, null, Encoding.UTF8, null)
        {
            Hints = [];
            AgentName = null;
            IsNewAgent = false;
            IsMessageDone = false;
            IsChatComplete = false;
            RequestChatRoomChange = false;
            RequestedChatRoom = "";
            RequestedChatRoomContext = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentStreamingContent"/> class with specified values.
        /// </summary>
        /// <param name="hints">The dictionary of hints for the streaming content.</param>
        /// <param name="agentName">The name of the agent sending the message.</param>
        /// <param name="isNewAgent">Indicates if this is a new agent's message.</param>
        /// <param name="isMessageDone">Indicates if the current message is complete.</param>
        /// <param name="isChatComplete">Indicates if the entire chat conversation is complete.</param>
        public AgentStreamingContent(
            Dictionary<string, object> hints,
            string? agentName = null,
            bool isNewAgent = false,
            bool isMessageDone = false,
            bool isChatComplete = false)
            : base(null, null, null, 0, null, Encoding.UTF8, null) // Adjust default base values if needed.
        {
            Hints = hints;
            AgentName = agentName;
            IsNewAgent = isNewAgent;
            IsMessageDone = isMessageDone;
            IsChatComplete = isChatComplete;
            RequestChatRoomChange = false;
            RequestedChatRoom = "";
            RequestedChatRoomContext = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentStreamingContent"/> class by copying values from an existing instance.
        /// </summary>
        /// <param name="other">The instance to copy values from.</param>
        public AgentStreamingContent(AgentStreamingContent other)
            : base(other.Role, other.Content, null, 0, null, other.Encoding, null) // Copy base properties.
        {
            // Create a new dictionary for hints and copy all entries.
            Hints = new Dictionary<string, object>(other.Hints);
            AgentName = other.AgentName;
            IsNewAgent = other.IsNewAgent;
            IsMessageDone = other.IsMessageDone;
            IsChatComplete = other.IsChatComplete;
            RequestChatRoomChange = other.RequestChatRoomChange;
            RequestedChatRoom = "";
            RequestedChatRoomContext = "";
        }
    }
}
