using Logic.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Protocols
{
    public class Guard : Event
    {
        public override EventType Type { get { return EventType.Guard; } }

        public int PlayerIndex { get; set; }

        public int AttackerPlayerIndex { get; set; }
    }
}
