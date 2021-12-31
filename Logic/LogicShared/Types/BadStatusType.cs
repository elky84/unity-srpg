using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum BadStatusType
    {
        [Description("없음")]
        NONE,

        [Description("띄우기")]
        AIR,

        [Description("당기기")]
        PULL,

        [Description("다운")]
        DOWN,

        [Description("밀치기")]
        KNOCKBACK,
    }
}
