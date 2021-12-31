using Logic.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Protocols
{
    public class SkillEffect
    {
        public EffectType EffectType { get; set; }

        public List<string> Effects { get; set; }
    }

    public class SkillCondition
    {
        public ConditionType ConditionType { get; set; }

        public List<string> Conditions { get; set; }
    }

    public class SkillTarget
    {
        public TargetType TargetType { get; set; }

        public List<string> Targets { get; set; }
    }

    public class Skill
    {
        [JsonProperty]
        public int SkillId { get; set; }

        [JsonProperty]
        public int CooltimeConfig { get; set; }

        [JsonProperty]
        public int Cooltime { get; set; }

        [JsonProperty]
        public List<SkillEffect> Effects { get; set; }

        [JsonProperty]
        public List<SkillCondition> Conditions { get; set; }

        [JsonProperty]
        public SkillTarget Target { get; set; }

        [JsonProperty]
        public SkillType SkillType { get; set; }
    }
}
