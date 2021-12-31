using Logic.Types;
using System.Collections.Generic;

namespace Logic.Protocols
{
    public class NormalAttack : Event
    {
        public override EventType Type { get { return EventType.NormalAttack; } }

        public int PlayerIndex { get; set; }

        public List<int> TargetPlayers { get; set; }

        public bool Critical { get; set; }
    }
}
