﻿﻿namespace YamlConfigurations.Validations
{
    public class ValidationError
    {
        public string Message { get; }
        public string Location { get; }
        public int? LineNumber { get; }
        public int? CharPosition { get; }

        public ValidationError(string message, string location, int? lineNumber = null, int? charPosition = null)
        {
            Message = message;
            Location = location;
            LineNumber = lineNumber;
            CharPosition = charPosition;
        }

        public override string ToString()
        {
            var pos = (LineNumber.HasValue && CharPosition.HasValue)
                ? $" (Line {LineNumber}, Char {CharPosition})"
                : "";
            return $"{Message} : {Location}{pos}";
        }
    }
}
