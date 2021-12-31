using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum TimingParameterType
    {
        [Description("전")]
        BEFORE,

        [Description("후")]
        AFTER,
    }
}
