using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ConditionType
    {
        [Description("반격")]
        COUNTER_ATTACK,

        [Description("상태이상")]
        BAD_STATUS,

        [Description("데미지")]
        DAMAGE,

        [Description("버프")]
        BUFF,

        [Description("아군버프")]
        ALLY_BUFF,

        [Description("적군버프")]
        ENEMY_BUFF,

        [Description("체력이하")]
        HP_LESS,

        [Description("체력비율")]
        HP_RATIO,

        [Description("최대체력비율데미지")]
        DAMAGE_BY_HP_RATIO,

        [Description("죽이고난뒤")]
        AFTER_KILL,

        [Description("일반공격")]
        NORMAL_ATTACK,

        [Description("확률")]
        PROBABILITY,

        [Description("죽는공격")]
        DEADLY_ATTACK,

        [Description("피격자")]
        BE_ATTACKED,

        [Description("피격자상태")]
        BE_ATTACKED_BUFF,

        [Description("공격자")]
        ATTACKER,

        [Description("공격자적군")]
        ENEMY,

        [Description("공격자아군")]
        ALLY,

        [Description("피격자클래스")]
        BE_ATTACKED_CLASS_TYPE,

        [Description("공격자클래스")]
        ATTACKER_CLASS_TYPE,

        [Description("콤보발동")]
        COMBO_ATTACK,

        [Description("액티브스킬로처치")]
        KILL_BY_ACTIVE_SKILL,
    }

}
