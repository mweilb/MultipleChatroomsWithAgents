namespace SemanticKernelExtension.Agents
{
    public class RuleInfoManager
    {
        private readonly Dictionary<KeyValuePair<string, string>, bool> _yieldOnRoomChanges = [];
        private readonly Dictionary<KeyValuePair<string, string>, string> _yieldCanceledNames = [];
        private readonly Dictionary<KeyValuePair<string, string>, string> _instructions = [];

        public void InsertRuleInfo(string agentName, string ruleName, string instructions, bool yieldOnChange, string yieldCanceledName)
        {
            var key = new KeyValuePair<string, string>(agentName, ruleName.ToLowerInvariant());
            if (!_yieldOnRoomChanges.ContainsKey(key))
                _yieldOnRoomChanges.Add(key, yieldOnChange);
            if (!_yieldCanceledNames.ContainsKey(key))
                _yieldCanceledNames.Add(key, yieldCanceledName);
            if (!_instructions.ContainsKey(key))
                _instructions.Add(key, instructions);
        }

        public bool TryGetYieldOnRoomChange(string agentName, string ruleName, out bool yieldOnRoomChange)
        {
            var key = new KeyValuePair<string, string>(agentName, ruleName.ToLowerInvariant());
            return _yieldOnRoomChanges.TryGetValue(key, out yieldOnRoomChange);
        }

        public bool TryGetYieldCanceledName(string agentName, string ruleName, out string? yieldCanceledName)
        {
            var key = new KeyValuePair<string, string>(agentName, ruleName.ToLowerInvariant());
            return _yieldCanceledNames.TryGetValue(key, out yieldCanceledName);
        }

        public bool TryGetInstructions(string agentName, string ruleName, out string? instructions)
        {
            var key = new KeyValuePair<string, string>(agentName, ruleName.ToLowerInvariant());
            return _instructions.TryGetValue(key, out instructions);
        }
    }
}
