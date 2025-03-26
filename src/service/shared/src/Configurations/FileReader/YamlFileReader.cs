using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MultiAgents.Configurations.Validations;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MultiAgents.Configurations.FileReader
{
    public class YamlFileReader
    {
        // Helper method to setup and validate a YamlMultipleChatRooms instance.
        private static void SetupAndValidate(YamlMultipleChatRooms experience, string yamlText, Kernel kernel, int embeddedSize, ILoggerFactory loggerFactory)
        {
            experience.Yaml = yamlText;
            experience.Setup(kernel, embeddedSize, loggerFactory);

            var validator = new YamlChatRoomsValidator();
            var errors = validator.Validate(experience);
            if (errors.Any())
            {
                List<string> errorResults = [];
                foreach (var error in errors)
                {
                    if (error != null)
                    {
                        errorResults.Add(error.ToString());
                    }
                }
           
                // Assign the error string to the Errors property of YamlMultipleChatRooms
                experience.Errors = errorResults;
            }
        }

        // Deserialize a YAML file into a dictionary of YamlMultipleChatRooms.
        public static Dictionary<string, YamlMultipleChatRooms> Read(string yamlFilePath, Kernel kernel, int embeddedSize, ILoggerFactory loggerFactory)
        {
            string yamlText = File.ReadAllText(yamlFilePath);
            var deserializer = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .Build();

            var experienceDict = deserializer.Deserialize<Dictionary<string, YamlMultipleChatRooms>>(yamlText);
            foreach (var (name, experience) in experienceDict)
            {
                experience.Name = name;
                SetupAndValidate(experience, yamlText, kernel, embeddedSize, loggerFactory);
            }
            return experienceDict;
        }

        // Deserialize a YAML file into a single YamlRoomConfig and wrap it in a YamlMultipleChatRooms instance.
        public static Dictionary<string, YamlMultipleChatRooms> ReadRoom(string yamlFilePath, Kernel kernel, int embeddedSize, ILoggerFactory loggerFactory)
        {
            string yamlText = File.ReadAllText(yamlFilePath);
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

                SetupAndValidate(experience, yamlText, kernel, embeddedSize, loggerFactory);
                dictExperiences.Add(yamlRoomConfig.Name, experience);
            }

            return dictExperiences;
        }
    }
}
