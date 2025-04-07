 

namespace YamlConfigurations.Librarians
{
    public class YamLibrarians
    {
        public List<YamlInstanceOfAgentConfig> ActiveLibrarians { get; set; } = [];
        public List<YamlInstanceOfAgentConfig> NotActiveLibrarians { get; set; } = [];
        public string RoomName { get; set; } = string.Empty;
        public string RoomEmoji { get; set; } = string.Empty;
 
    }
}
