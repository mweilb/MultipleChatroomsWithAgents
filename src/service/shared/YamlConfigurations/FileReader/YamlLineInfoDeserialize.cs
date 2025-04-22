using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;


namespace YamlConfigurations.FileReader
{

    // Wrap the default object deserializer
    public class YamlLineInfoDeserialize : INodeDeserializer
    {
        private readonly INodeDeserializer _inner;
        public YamlLineInfoDeserialize(INodeDeserializer inner) => _inner = inner;

        public bool Deserialize(
            IParser parser,
            Type expectedType,
            Func<IParser, Type, object?> nestedObjectDeserializer,
            out object? value,
            ObjectDeserializer rootDeserializer)
        {
            // Try to get the start mark from the current event, but do not advance unless necessary
            Mark start = Mark.Empty;
            ParsingEvent? parsingEvent = parser.Current as ParsingEvent;
            if (parsingEvent == null)
            {
                if (parser.MoveNext())
                {
                    parsingEvent = parser.Current as ParsingEvent;
                }
            }
            if (parsingEvent != null)
            {
                start = parsingEvent.Start;
            }

            // Let the default deserializer do its job
            if (!_inner.Deserialize(parser, expectedType, nestedObjectDeserializer, out value, rootDeserializer))
            {
                return false;
            }

            // Set line info if applicable
            if (value is YamlLineInfo info)
            {
                info.StartLine = start.Line;
                info.StartColumn = start.Column;
            }
            return true;
        }
    }
}
