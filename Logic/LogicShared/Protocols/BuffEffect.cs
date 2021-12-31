using Logic.Types;
using System.Collections.Generic;

namespace Logic.Protocols
{
    public class BuffEffect : Event
    {
        public override EventType Type { get { return EventType.BuffEffect; } }

        public List<int> TargetPlayers { get; set; }

        public Buff Buff { get; set; }
    }
}
