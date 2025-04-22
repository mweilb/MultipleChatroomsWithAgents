﻿namespace YamlConfigurations.Validations
{
    public class ValidationError(string message,string location, YamlLineInfo info)
    {
        public string Message { get; } = message;
        public string Location { get;  } = location;
        public YamlLineInfo Info { get; } = info;
        public long LineNumber { get; } = info.StartLine;
        public long CharPosition { get; } = info.StartColumn;
    }
 
}
