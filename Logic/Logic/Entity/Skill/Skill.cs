using Logic.Types;
using Logic.Util;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

namespace Logic.Entity
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

    [JsonObject(MemberSerialization.OptIn)]
    public class Skill
    {
        [JsonProperty]
        public int SkillId { get; set; }

        [JsonProperty]
        public SkillType SkillType { get; set; }

        [JsonProperty]
        public List<SkillEffect> Effects { get; set; } = new List<SkillEffect>();

        [JsonProperty]
        public List<SkillCondition> Conditions { get; set; } = new List<SkillCondition>();

        [JsonProperty]
        public SkillTarget Target { get; set; } = new SkillTarget();

        [JsonProperty]
        public int CooltimeConfig { get; set; }

        [JsonProperty]
        public int Cooltime { get; set; }

        public string ToConditionLog()
        {
            return string.Format("[Id:{0}, Type:{1}, Condition:{2}]", SkillId, SkillType, string.Join(",", Conditions.ConvertAll(x => x.ConditionType.ToString()).ToArray()));
        }

        public string ToEffectLog()
        {
            return string.Format("[Id:{0}, Type:{1}, Effect:{2}, {3}, {4}]", SkillId, SkillType, string.Join(",", Effects.ConvertAll(type => type.EffectType.ToString()).ToArray()), Target.TargetType, CooltimeConfig);
        }

        public Protocols.Skill ToProtocol()
        {
            return new Protocols.Skill
            {
                SkillId = SkillId,
                SkillType = SkillType,
                CooltimeConfig = CooltimeConfig,
                Cooltime = Cooltime,
                Effects = Effects.ConvertAll(effect => new Protocols.SkillEffect { EffectType = effect.EffectType, Effects = effect.Effects }),
                Conditions = Conditions.ConvertAll(condition => new Protocols.SkillCondition { ConditionType = condition.ConditionType, Conditions = condition.Conditions }),
                Target = new Protocols.SkillTarget { TargetType = Target.TargetType, Targets = Target.Targets }
            };
        }
    }
}
