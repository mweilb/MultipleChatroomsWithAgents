namespace YamlConfigurations.Validations
{
    public interface IValidationPass
    {
        IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config);
    }
}
