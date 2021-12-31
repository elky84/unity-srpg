using Logic.Types;
using System;
using System.Collections.Generic;

namespace Logic.Protocols
{
    public class ComboAttack : Event
    {
        public override EventType Type { get { return EventType.ComboAttack; } }

        public int PlayerIndex { get; set; }

        public List<int> TargetPlayers { get; set; }

        public int Count { get; set; }
    }
}
