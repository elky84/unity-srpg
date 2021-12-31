using Logic.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Protocols
{
    public class BuffEffectRemove : Event
    {
        public override EventType Type { get { return EventType.BuffEffectRemove; } }

        public int PlayerIndex { get; set; }

        public Buff Buff { get; set; }
    }
}
