
using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum ClassType
    {
        [Description("공격형")]
        ATTACK,

        [Description("방어형")]
        DEFENSE,

        [Description("지원형")]
        SUPPORT
    }

}
