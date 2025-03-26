 
using Microsoft.SemanticKernel.Agents;
 
namespace MultiAgents.SemanticKernel.Modifications
{
    public interface IAgentStrategies
    {
        /// <summary>
        /// Returns the room rule based on the current (or last) agent.
        /// </summary>
        /// <param name="lastAgent">The last agent in the conversation, if any.</param>
        /// <returns>A rule object with streaming strategies, or null if not found.</returns>
        (TerminationStreamingStrategy termination, SelectionStreamingStrategy selection) GetStreamingStrategies(Agent? lastAgent,string lastTermination, bool EnteringFirstTime);

    }
}
