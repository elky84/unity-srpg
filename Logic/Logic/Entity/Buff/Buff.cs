using Logic.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Buff
    {
        [JsonProperty]
        public BuffType Type { get; set; }

        [JsonProperty]
        public int RemainCooltime { get; set; }

        [JsonProperty]
        public int Value { get; set; }


        public Protocols.Buff ToProtocol()
        {
            return new Protocols.Buff
            {
                Type = Type,
                RemainCooltime = RemainCooltime,
                Value = Value
            };
        }
    }
}
