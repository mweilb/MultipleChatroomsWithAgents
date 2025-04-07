namespace YamlConfigurations.Validations
{
    public class ValidationError
    {
        public string Message { get; }
        public string Location { get; }

        public ValidationError(string message, string location)
        {
            Message = message;
            Location = location;
        }

        public override string ToString() => $"{Message} : {Location}";

    }
}
