using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum CompareOperatorType
    {
        [Description("이상")]
        MORE_THAN,

        [Description("초과")]
        OVER,

        [Description("이하")]
        LESS_THAN,

        [Description("미만")]
        UNDER,

        [Description("일치")]
        EQUAL,

        [Description("불일치")]
        NOT_EQUAL,
    }


}
