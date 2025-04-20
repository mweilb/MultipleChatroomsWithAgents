﻿﻿namespace YamlConfigurations.Validations
{
    public class ValidationError(string message, YamlLineInfo info)
    {
        public string Message { get; } = message;
        public long LineNumber { get; } = info.StartLine;
        public long CharPosition { get; } = info.StartColumn;
 
    }
}
