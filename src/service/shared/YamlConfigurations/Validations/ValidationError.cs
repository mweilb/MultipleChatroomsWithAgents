﻿﻿﻿﻿﻿﻿namespace YamlConfigurations.Validations
{
    public static class ValidationErrorKeywords
    {
        public const string StartRoom = "start-room";
        public const string Room = "room";
        public const string Agent = "agent";
        public const string Termination = "termination";
        public const string Moderation = "moderation";
        public const string Selection = "selection";
        public const string NameCollision = "name-collision";
        public const string Instructions = "instructions";
        public const string Rule = "rule";
        public const string Current = "current";
        public const string Next = "next";
        public const string RegexTermination = "regex-termination";
        public const string ConstantTermination = "constant-termination";
        public const string PromptTermination = "prompt-termination";
        public const string PromptSelect = "prompt-select";
        public const string SequentialSelection = "sequential-selection";
        public const string RoundRobinSelection = "round-robin-selection";
        public const string Syntax = "syntax";
    }

    public class ValidationError 
    {
        /// <summary>
        /// Single-word category describing the error type.
        /// </summary>
        public string Keyword { get; init; }
        public string Message { get; init; }
        public string Location { get; init; }
        public YamlLineInfo Info { get; init; }
        public long LineNumber { get; init; }
        public long CharPosition { get; init; }

        public ValidationError(string message, string location, YamlLineInfo info, string keyword)
        {
            Message = message;
            Location = location;
            Info = info;
            LineNumber = info.StartLine;
            CharPosition = info.StartColumn;
            Keyword = keyword;
        }
    }
 
}
