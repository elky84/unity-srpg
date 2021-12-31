

namespace Logic.Types
{
    public enum EventType
    {
        Unknown,

        // Timing Event
        GameStart,
        TeamTurn,
        PlayerTurn,
        NormalAttack,
        SkillInvoke,
        Dead,
        ComboAttack,
        ActiveSkill,
        PositionMove,
        Guard,
        GameEnd,

        // Status Change
        HpChange,
        BadStatus,
        BuffEffect,
        BuffEffectRemove,
    }
}
