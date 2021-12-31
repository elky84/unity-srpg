using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum SortType
    {
        [Description("오름차순")]
        ASCENDING,

        [Description("내림차순")]
        DESCENDING,
    }

}
