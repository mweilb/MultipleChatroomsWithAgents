﻿﻿﻿namespace YamlConfigurations.Validations
{
    public static class ValidationErrorKeywords
    {
        public const string Name = "Name";
        public const string Reference = "Reference";
        public const string Prompt = "Prompt";
        public const string Rule = "Rule";
        public const string Selection = "Selection";
        public const string Termination = "Termination";
        public const string Collision = "Collision";
        public const string Instructions = "Instructions";
    }

    public class ValidationError : IEquatable<ValidationError>
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as ValidationError);
        }

        public bool Equals(ValidationError? other)
        {
            if (other is null)
                return false;

            return Message == other.Message
                && Location == other.Location
                && LineNumber == other.LineNumber
                && CharPosition == other.CharPosition
                && Keyword == other.Keyword;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Message?.GetHashCode() ?? 0);
                hash = hash * 23 + (Location?.GetHashCode() ?? 0);
                hash = hash * 23 + LineNumber.GetHashCode();
                hash = hash * 23 + CharPosition.GetHashCode();
                hash = hash * 23 + (Keyword?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
 
}
