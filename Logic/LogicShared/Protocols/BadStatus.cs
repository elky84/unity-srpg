using Logic.Types;
using System.Collections.Generic;

namespace Logic.Protocols
{
    public class BadStatus : Event
    {
        public override EventType Type { get { return EventType.BadStatus; } }

        public int PlayerIndex { get; set; }

        public List<int> TargetPlayers { get; set; }

        public BadStatusType BadStatusType { get; set; }
    }
}
