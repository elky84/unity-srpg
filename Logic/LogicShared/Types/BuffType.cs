using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum BuffType
    {
        [Description("없음")]
        NONE,

        [Description("모든버프")]
        ALL,

        [Description("스턴")]
        STUN,

        [Description("타운트")]
        TAUNT,          //일반공격 대신 맞기

        [Description("히든")]
        HIDDEN,         //일반공격에서 제외

        [Description("공격력증가")]
        ATTACK_UP,

        [Description("공격속도증가")]
        ATTACK_SPEED_UP,

        [Description("화상")]
        BURN,
    }
}
