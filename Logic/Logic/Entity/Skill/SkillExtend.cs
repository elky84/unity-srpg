using Logic.Common;
using Logic.Types;
using Logic.TypesBehaviour;
using Logic.Util;
using System;
using System.Collections.Generic;

namespace Logic.Entity
{
    public static class SkillExtend
    {
        public static void AddSkill(Player player, string skillText, int skillId)
        {
            if (!string.IsNullOrEmpty(skillText))
            {
                var skill = skillText.ParseYamlSkill();
                if (skill != null)
                {
                    skill.SkillId = skillId;
                    if (skill.SkillType == SkillType.ACTIVE)
                    {
                        player.ActiveSkill.Add(skill);
                    }
                    else if (skill.SkillType == SkillType.PASSIVE)
                    {
                        player.PassiveSkill.Add(skill);
                    }
                }
            }
        }

        public static bool Setup(this Skill skill, Dictionary<string, string> dic)
        {
            if (dic.TryGetValue(SkillProperty.TYPE.GetDescription(), out string type))
            {
                var skillType = TypesUtil.FromDescription<SkillType>(type);
                if (skillType.HasValue)
                {
                    skill.SkillType = skillType.Value;
                }
                else
                {
                    LogUtil.WriteError("[INVALID_INPUT] 타입: {0}", type);
                }
            }

            if (dic.TryGetValue(SkillProperty.EFFECT.GetDescription(), out string effect))
            {
                foreach (var str in effect.Split('&'))
                {
                    var parseEffect = str.Parse();

                    EffectType? effectType = TypesUtil.FromDescription<EffectType>(parseEffect.Key);
                    if (effectType.HasValue)
                    {
                        skill.Effects.Add(new Entity.SkillEffect { EffectType = effectType.Value, Effects = parseEffect.Value });
                    }
                    else
                    {
                        LogUtil.WriteError("[INVALID_INPUT] 효과: {0}", parseEffect.Key);
                    }
                }
            }

            if (dic.TryGetValue(SkillProperty.CONDITION.GetDescription(), out string condition))
            {
                foreach (var str in condition.Split('&'))
                {
                    var parseCondition = str.Parse();
                    ConditionType? conditionType = TypesUtil.FromDescription<ConditionType>(parseCondition.Key);
                    if (conditionType.HasValue)
                    {
                        skill.Conditions.Add(new SkillCondition { ConditionType = conditionType.Value, Conditions = parseCondition.Value });
                    }
                    else
                    {
                        LogUtil.WriteError("[INVALID_INPUT] 조건: {0}", parseCondition.Key);
                    }
                }
            }

            if (false == dic.TryGetValue(SkillProperty.TARGET.GetDescription(), out string target))
            {
                target = TargetType.ENEMY.GetDescription() + "(1)";
            }

            {
                var parseTarget = target.Parse();
                TargetType? targetType = TypesUtil.FromDescription<TargetType>(parseTarget.Key);
                if (targetType.HasValue)
                {
                    skill.Target = new SkillTarget { TargetType = targetType.Value, Targets = parseTarget.Value };
                }
                else
                {
                    LogUtil.WriteError("[INVALID_INPUT] 타겟: {0}", parseTarget.Key);
                }
            }

            if (false == dic.TryGetValue(SkillProperty.COOLTIME.GetDescription(), out string cooltime))
            {
                cooltime = "0";
            }
            skill.CooltimeConfig = cooltime.ToInt();

            return true;
        }

        public static bool OnEffect(this Skill skill, Player player)
        {
            //TODO damage를 ref로 주고 받는 것이 맞는지부터 시작해서 검토 및 수정 필요
            foreach (var effect in skill.Effects)
            {
                if (false == effect.EffectType.OnEffect(effect.Effects, skill, player))
                {
                    return false;
                }
            }

            skill.Cooltime = skill.CooltimeConfig;

            LogUtil.WriteSkillEffect("[효과적용] [Player:{0}] [Skill:{1}]", player.ToLog(), skill.ToEffectLog());
            return true;
        }


        public static bool ConditionCheck(this Skill skill, TimingType timingType, Player player)
        {
            if (skill.Cooltime > 0)
            {
                return false;
            }


            foreach (var condition in skill.Conditions)
            {
                if (false == condition.ConditionType.ConditionCheck(condition.Conditions, timingType, player))
                {
                    return false;
                }
            }

            LogUtil.WriteInfo("[조건만족] [Player:{0}] [Skill:{1}]", player.ToLog(), skill.ToConditionLog());
            return true;
        }
    }
}
