
using YamlConfigurations.Validations;
using YamlDotNet.Serialization;
 

namespace YamlConfigurations.FileReader
{
    public class YamlFileReader
    {
        // Helper method to setup and validate a YamlMultipleChatRooms instance.
        private static void SetupAndValidate(YamlMultipleChatRooms experience, string yamlText)
        { 
            experience.Yaml = yamlText;
            experience.ApplyParentOverride();

            var validator = new YamlChatRoomsValidator();
            var errors = validator.Validate(experience, yamlText);
            if (errors.Any())
            {
                // Assign the error objects directly to the Errors property of YamlMultipleChatRooms
                experience.Errors = errors.ToList();
            }
        }

        // Deserialize a YAML file into a dictionary of YamlMultipleChatRooms.
        public static Dictionary<string, YamlMultipleChatRooms> Read(string yamlFilePath)
        {
            string yamlText = File.ReadAllText(yamlFilePath);
           
            //try the first format
            try
            {
                Dictionary<string, YamlMultipleChatRooms> experienceDict = ReadExperienceFormat(yamlText);
                return experienceDict;
            }
            catch { } 
            { 
            }
            
            //try the second format
            try
            {
                Dictionary<string, YamlMultipleChatRooms> dictExperiences = ReadIndivualRoomFormat(yamlText);

                return dictExperiences;
            }
            catch {
            
            }

            return [];
            
        }

        private static Dictionary<string, YamlMultipleChatRooms> ReadIndivualRoomFormat(string yamlText)
        {
            var deserializer = new DeserializerBuilder()
              .IgnoreUnmatchedProperties()
              .Build();

            var yamlRoomConfig = deserializer.Deserialize<YamlRoomConfig>(yamlText);
            var dictExperiences = new Dictionary<string, YamlMultipleChatRooms>();

            if (yamlRoomConfig != null)
            {
                var experience = new YamlMultipleChatRooms
                {
                    Name = yamlRoomConfig.Name,
                    Emoji = yamlRoomConfig.Emoji,
                    Rooms = new Dictionary<string, YamlRoomConfig>
                    {
                        { yamlRoomConfig.Name, yamlRoomConfig }
                    }
                };

                SetupAndValidate(experience, yamlText);
                dictExperiences.Add(yamlRoomConfig.Name, experience);
            }

            return dictExperiences;
        }

        private static Dictionary<string, YamlMultipleChatRooms> ReadExperienceFormat(string yamlText)
        {
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            var experienceDict = deserializer.Deserialize<Dictionary<string, YamlMultipleChatRooms>>(yamlText);
            foreach (var (name, experience) in experienceDict)
            {
                experience.Name = name;
                SetupAndValidate(experience, yamlText);
            }

            return experienceDict;
        }

    }
}
