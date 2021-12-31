using Logic.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Logic.Protocols
{
    public class Event
    {
        public virtual EventType Type { get; set; }

        public int Sequence { get; set; }

        [JsonExtensionData]
        public JObject ExtensionData { get; set; }
    }
}
