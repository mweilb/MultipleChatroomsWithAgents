using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
namespace YamlConfigurations.FileReader
{
 
   public class YamlStringWithLocationConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) =>
            type == typeof(YamlStringWithLocation);

 

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
        {
            // Consume the scalar event, capturing its text + position
            var scalar = parser.Consume<Scalar>();
            return new YamlStringWithLocation
            {
                Value = scalar.Value ?? string.Empty,
                StartLine = scalar.Start.Line,
                StartColumn = scalar.Start.Column
            };
        }
 

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
        {
            // If you ever serialize back, just write the raw string
            var src = (YamlStringWithLocation?)value;
            emitter.Emit(new Scalar(src?.Value??""));
        }
    }
}
