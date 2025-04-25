
using YamlConfigurations.Validations;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NodeDeserializers;


namespace YamlConfigurations.FileReader
{
    public class YamlFileReader
    {
        // Helper method to setup and validate a YamlMultipleChatRooms instance.
        private static void SetupAndValidate(YamlMultipleChatRooms experience)
        {
            experience.ApplyParentOverride();

            var validator = new YamlChatRoomsValidator();
            var errors = validator.Validate(experience);
            if (errors.Any())
            {
                // Assign the error objects directly to the Errors property of YamlMultipleChatRooms
                experience.Errors = [.. errors];
            }
        }

        // Deserialize a YAML file into a dictionary of YamlMultipleChatRooms.
        public static (bool, string, Dictionary<string, YamlMultipleChatRooms>) ReadFile(string yamlFilePath)
        {
            string yamlText = File.ReadAllText(yamlFilePath);
            var (synataxValid, experienceDict) = ReadFromString(yamlText);
            return (synataxValid, yamlText,experienceDict);
        }

        // Deserialize YAML content from a string into a dictionary of YamlMultipleChatRooms.
        public static (bool, Dictionary<string, YamlMultipleChatRooms>) ReadFromString(string yamlText)
        {
            // Run simple linter first
            var lintErrors = YamlSimpleLinter.Lint(yamlText);

            static List<ValidationError> ToValidationErrors(List<YamlLintResult> lintErrors)
            {
                var result = new List<ValidationError>();
                foreach (var err in lintErrors)
                {
                    if (err.Message == "Trailing whitespace detected.")
                        continue;
                    result.Add(new ValidationError(err.Message, "", err.LineInfo, ValidationErrorKeywords.Syntax));
                }
                return result;
            }

            try
            {
                var deserializer = new DeserializerBuilder()
               .WithTypeConverter(new YamlStringWithLocationConverter())
               .WithNodeDeserializer(
                   inner => new YamlLineInfoDeserialize(inner),
                   s => s.InsteadOf<ObjectNodeDeserializer>())
               .IgnoreUnmatchedProperties()
               .Build();

                var experienceDict = deserializer.Deserialize<Dictionary<string, YamlMultipleChatRooms>>(yamlText);

                var lintValidationErrors = ToValidationErrors(lintErrors);

                foreach (var (name, experience) in experienceDict)
                {
                    experience.Name = name;
                    experience.Yaml = yamlText;
                    if (experience.Rooms == null || experience.Rooms.Count == 0)
                    {
                        experience.Rooms = [];
                        experience.Rooms.Add(name, experience);
                    }
                    SetupAndValidate(experience);

                    // Add lint errors to Errors list
                    if (lintValidationErrors.Count > 0)
                    {
                        if (experience.Errors == null)
                            experience.Errors = [];
                        experience.Errors.AddRange(lintValidationErrors);
                    }
                }
                return (lintValidationErrors.Count() == 0, experienceDict);
            }
            catch (YamlException)
            {
                Dictionary<string, YamlMultipleChatRooms> experienceDict = [];
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                string errorName = $"Error-{timestamp}";

                experienceDict.Add(errorName, new YamlMultipleChatRooms
                {
                    Name = errorName,
                    Yaml = yamlText,
                    Errors  = ToValidationErrors(lintErrors)
                });
                return (false, experienceDict);
            }
        }
    }
}
