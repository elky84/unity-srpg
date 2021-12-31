using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum TeamType
    {
        [Description("BLUE")]
        BLUE,

        [Description("RED")]
        RED
    }

}
