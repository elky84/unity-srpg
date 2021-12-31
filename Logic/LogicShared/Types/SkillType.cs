
using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum SkillType
    {
        [Description("액티브")]
        ACTIVE,

        [Description("패시브")]
        PASSIVE,
    }
}
