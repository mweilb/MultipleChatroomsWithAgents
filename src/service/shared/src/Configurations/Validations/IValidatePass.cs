

 
namespace MultiAgents.Configurations.Validations
{
    public interface IValidationPass
    {
        IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config);
    }
}
