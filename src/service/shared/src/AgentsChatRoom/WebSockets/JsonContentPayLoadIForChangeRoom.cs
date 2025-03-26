using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multi_agents_shared.src.AgentsChatRoom.WebSockets
{
    internal class JsonContentPayLoadIForChangeRoom
    {
        public string Group { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}
