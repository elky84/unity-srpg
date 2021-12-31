using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum SkillProperty
    {
        [Description("타입")]
        TYPE,

        [Description("조건")]
        CONDITION,

        [Description("효과")]
        EFFECT,

        [Description("대상")]
        TARGET,

        [Description("쿨타임")]
        COOLTIME,
    }

}
