namespace YamlConfigurations.Validations
{
    public interface IValidationPass
    {
        void Validate(YamlMultipleChatRooms config, IList<ValidationError> errors);
    }
}
