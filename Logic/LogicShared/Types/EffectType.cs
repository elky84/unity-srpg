using Logic.Common;

namespace Logic.Types
{
    [DescriptiveEnumEnforcement(DescriptiveEnumEnforcementAttribute.EnforcementTypeEnum.ThrowException)]
    public enum EffectType
    {
        [Description("공격속도증가")]
        ATTACK_SPEED_INCREASE,

        [Description("공격속도감소")]
        ATTACK_SPEED_DECREASE,

        [Description("공격력증가")]
        ATTACK_INCREASE,

        [Description("버프갯수비례공격력추가")]
        ATTACK_INCREASE_BY_BUFF_COUNT,

        [Description("상태이상")]
        BAD_STATUS,

        [Description("대신피해받음")]
        INSTEAD,

        [Description("콤보발동")]
        CREATE_COMBO,

        [Description("콤보확률증가")]
        COMBO_PROBABILITY,

        [Description("데미지경감")]
        DAMAGE_ALLEVIATE,

        [Description("추가공격")]
        ADDITIONAL_ATTACK,

        [Description("일반공격")]
        NORMAL_ATTACK,

        [Description("일반공격배수")]
        NORMAL_ATTACK_RATIO,

        [Description("일반공격횟수")]
        NORMAL_ATTACK_COUNT,

        [Description("버프")]
        BUFF,

        [Description("체력회복")]
        RECOVERY_HP,

        [Description("체력에따른회복")]
        RECOVERY_HP_BY_HP,

        [Description("버프감소")]
        DECREASE_BUFF,

        [Description("데미지")]
        DAMAGE,

        [Description("데미지횟수")]
        DAMAGE_COUNT,

        [Description("데미지비율")]
        DAMAGE_RATIO,

        [Description("클래스에따른데미지")]
        DAMAGE_BY_CLASS,

        [Description("상태에따른데미지")]
        DAMAGE_BY_BUFF,

        [Description("체력에따른데미지")]
        DAMAGE_BY_HP,

        [Description("스킬쿨타임감소")]
        SKILL_COOLTIME_DECREASE,

        [Description("스킬쿨타임증가")]
        SKILL_COOLTIME_INCREASE,

        [Description("액티브쿨타임감소")]
        ACTIVE_COOLTIME_DECREASE,

        [Description("액티브쿨타임증가")]
        ACTIVE_COOLTIME_INCREASE,

        [Description("콤보쿨타임감소")]
        COMBO_COOLTIME_DECREASE,

        [Description("존재여부에따른데미지")]
        DAMAGE_BY_EXIST,

        [Description("순서대로데미지")]
        DAMAGE_BY_ORDER,

        [Description("타겟순서변경")]
        CHANGE_ORDER,

        [Description("모든버프삭제")]
        REMOVE_ALL_BUFF,

        [Description("콤보데미지")]
        COMBO_DAMAGE,

        [Description("화상")]
        BURN,
    }
}
