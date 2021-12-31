using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum TargetType
    {
        [Description("자신")]
        SELF,

        [Description("무작위적군")]
        ENEMY,

        [Description("무작위아군")]
        ALLY,

        [Description("모든적군")]
        ENEMY_ALL,

        [Description("모든아군")]
        ALLY_ALL,

        [Description("적군")]
        ENEMY_EVERYONE,

        [Description("아군")]
        ALLY_EVERYONE,

        [Description("특정상태아군")]
        ALLY_BY_BUFF,

        [Description("특정상태적군")]
        ENEMY_BY_BUFF,

        [Description("자신제외아군")]
        ALLY_ALL_EXCEPT_SELF,

        [Description("체력이가장낮은적군")]
        LOWHP_ENEMY,

        [Description("체력이가장높은적군")]
        HIGHHP_ENEMY,

        [Description("체력순서적군")]
        ENEMY_ORDER_BY_HP,

        [Description("클래스적군")]
        CLASS_ENEMY,

        [Description("클래스아군")]
        CLASS_ALLY,

        [Description("클래스제외적군")]
        WITHOUT_CLASS_ENEMY,

        [Description("클래스제외아군")]
        WITHOUT_CLASS_ALLY,

        [Description("누구든")]
        ANY,

        [Description("특정아군")]
        ALLY_SELECT,

        [Description("특정적군")]
        ENEMY_SELECT,
    }

}
