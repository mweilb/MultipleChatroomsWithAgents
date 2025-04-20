using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YamlConfigurations
{
    public class YamlStringWithLocation : YamlLineInfo
    {
        // This is the scalar’s text
        public string? Value { get; set; } = string.Empty;

        public override string ToString() => Value ?? string.Empty;

        public static implicit operator string?(YamlStringWithLocation? src)
            => src?.Value;
    }
}
