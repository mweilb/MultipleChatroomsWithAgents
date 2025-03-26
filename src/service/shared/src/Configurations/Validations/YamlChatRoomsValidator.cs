 
namespace MultiAgents.Configurations.Validations
{
    public class YamlChatRoomsValidator
    {
        private readonly List<IValidationPass> _validationPasses;

        public YamlChatRoomsValidator()
        {
            _validationPasses = new List<IValidationPass>
        {
            new PromptNotEmptyValidation(),
            new AgentInstructionsValidation(),
            new RuleCompletenessValidation(),
            new NextRoomValidation(),
            new AgentReferenceValidation(),
            new MessagesPresetFiltersValidation(),
            new TerminationPresetValidation(),
            new ModerationPromptNotEmptyValidation()
            // Add additional validations here as needed.
        };
        }

        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();
            foreach (var pass in _validationPasses)
            {
                errors.AddRange(pass.Validate(config));
            }
            return errors;
        }
    }

}
