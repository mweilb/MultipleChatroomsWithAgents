﻿﻿namespace YamlConfigurations.Validations
{
    public class ValidationError : IEquatable<ValidationError>
    {
        public string Message { get; init; }
        public string Location { get; init; }
        public YamlLineInfo Info { get; init; }
        public long LineNumber { get; init; }
        public long CharPosition { get; init; }

        public ValidationError(string message, string location, YamlLineInfo info)
        {
            Message = message;
            Location = location;
            Info = info;
            LineNumber = info.StartLine;
            CharPosition = info.StartColumn;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ValidationError);
        }

        public bool Equals(ValidationError other)
        {
            if (other is null)
                return false;

            return Message == other.Message
                && Location == other.Location
                && LineNumber == other.LineNumber
                && CharPosition == other.CharPosition;
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
                return hash;
            }
        }
    }
 
}
