using Logic.Types;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Logic.Protocols
{
    public class SkillInvoke : Event
    {
        public override EventType Type { get { return EventType.SkillInvoke; } }

        public SkillType SkillType { get; set; }

        public int PlayerIndex { get; set; }

        public List<int> TargetPlayers { get; set; }

        public Skill Skill { get; set; }
    }
}