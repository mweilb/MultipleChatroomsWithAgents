﻿﻿﻿﻿ 
using System.Linq;
namespace YamlConfigurations.Validations
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
            new SelectionValidation(),
            new TerminationValidation(),
            new ModerationPromptNotEmptyValidation(),
            new AgentNameValidation(),
            new RoomNameValidation()
            // Add additional validations here as needed.
        };
        }

        public IEnumerable<ValidationError> Validate(YamlMultipleChatRooms config)
        {
            var errors = new List<ValidationError>();
            foreach (var pass in _validationPasses)
            {
                pass.Validate(config, errors);
            }
            var seen = new HashSet<string>();
            var unique = new List<ValidationError>();
            foreach (var error in errors)
            {
                var key = $"{error.Message}|{error.LineNumber}|{error.CharPosition}";
                if (seen.Add(key))
                {
                    unique.Add(error);
                }
            }
            return unique;
        }
    }

}
