using Logic.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Protocols
{
    public class HpChange : Event
    {
        public override EventType Type { get { return EventType.HpChange; } }

        public int PlayerIndex { get; set; }

        public int DestPlayerIndex { get; set; }

        public double Value { get; set; }

        public EffectType EffectType { get; set; }
    }
}
