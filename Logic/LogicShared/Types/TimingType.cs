using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum TimingType
    {
        [Description("일반공격_전")]
        NORMAL_ATTACK_BEFORE,

        [Description("일반공격_후")]
        NORMAL_ATTACK_AFTER,

        [Description("액티브스킬_전")]
        ACTIVE_SKILL_BEFORE,

        [Description("액티브스킬_후")]
        ACTIVE_SKILL_AFTER,

        [Description("콤보공격")]
        COMBO_ATTACK,

        [Description("액티브스킬로처치")]
        KILL_BY_ACTIVE_SKILL,

        [Description("죽이고난뒤")]
        DEATH_AFTER,

        [Description("죽는공격")]
        DEADABLE_ATTACK,

        [Description("반격")]
        COUNTER_ATTACK,
    }
}
