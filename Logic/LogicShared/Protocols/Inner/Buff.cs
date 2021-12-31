using Logic.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Protocols
{
    public class Buff
    {
        public BuffType Type { get; set; }

        public int RemainCooltime { get; set; }

        public int Value { get; set; }
    }
}
