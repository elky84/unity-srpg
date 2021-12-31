using Logic.Entity;
using Logic.Types;
using Logic.Util;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Logic.TypesBehaviour
{
    static class ConditionTypeBehaviour
    {
        public static void InvalidParameter(ConditionType conditionType, List<string> parameters, TimingType timingType, Player player)
        {
            LogUtil.WriteError("[Invalid Parameter] conditionType:{0}, timingType:{1}, player:{2}, parameters:{3}", conditionType, timingType, player.ToLog(), string.Join(",", parameters));
        }

        public static bool ConditionCheck(this ConditionType conditionType, List<string> parameters, TimingType timingType, Player player)
        {
            switch (conditionType)
            {
                case ConditionType.COUNTER_ATTACK:
                    {
                        if (timingType == TimingType.COUNTER_ATTACK)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.BAD_STATUS:
                    {
                        var badStatusType = TypesUtil.FromName<BadStatusType>(parameters[0]);
                        if (!badStatusType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        return player.BadStatusType == badStatusType.Value;
                    }
                case ConditionType.DAMAGE_BY_HP_RATIO:
                    {
                        double ratio = parameters[0].ToDouble() * 100;
                        var compareOperatorType = TypesUtil.FromDescription<CompareOperatorType>(parameters[0].ExtractKorean());
                        if (!compareOperatorType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        double ratioByHp = ratio / player.HpRatio;
                        switch (compareOperatorType.Value)
                        {
                            case CompareOperatorType.MORE_THAN:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack >= ratioByHp)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.LESS_THAN:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack <= ratioByHp)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.OVER:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack > ratioByHp)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.UNDER:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack < ratioByHp)
                                        return true;
                                }
                                return false;
                            default:
                                return false;
                        }
                    }
                case ConditionType.BE_ATTACKED:
                    {
                        var targetType = TypesUtil.FromDescription<TargetType>(parameters[0]);
                        if (!targetType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        var targets = player.GameState.GetTargets(player, targetType.Value, new List<string> { parameters.Count > 1 ? parameters[1] : "1" }, out var needSelection);
                        if (targets.Contains(null))
                        {
                            return false;
                        }

                        foreach (var target in targets)
                        {
                            if (player.GameState.TargetPlayers.Contains(target) == false)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                case ConditionType.NORMAL_ATTACK:
                    {
                        var timingParameterType = TypesUtil.FromDescription<TimingParameterType>(parameters[0]);
                        if (!timingParameterType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        if (timingParameterType == TimingParameterType.BEFORE && timingType == TimingType.NORMAL_ATTACK_BEFORE)
                        {
                            return true;
                        }
                        if (timingParameterType == TimingParameterType.AFTER && timingType == TimingType.NORMAL_ATTACK_AFTER)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.ATTACKER:
                    {
                        var targetType = TypesUtil.FromDescription<TargetType>(parameters[0]);
                        var targets = player.GameState.GetTargets(player, targetType.Value, null, out var needSelection);
                        if (targets.Contains(null))
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        foreach (var target in targets)
                        {
                            if (target == player.GameState.CurrentPlayer)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                case ConditionType.ALLY:
                    {
                        if (player.TeamType == player.GameState.CurrentTeam.TeamType)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.ENEMY:
                    {
                        if (player.TeamType != player.GameState.CurrentTeam.TeamType)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.COMBO_ATTACK:
                    {
                        if (timingType == TimingType.COMBO_ATTACK)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.BE_ATTACKED_BUFF:
                    {
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);
                        if (!buffType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            if (targetPlayer.HasBuff(buffType.Value))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                case ConditionType.BE_ATTACKED_CLASS_TYPE:
                    {
                        ClassType? classType = TypesUtil.FromDescription<ClassType>(parameters[0]);
                        if (!classType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            if (targetPlayer.PlayerData.Type == classType.Value)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                case ConditionType.DAMAGE:
                    {
                        int receiveDamage = parameters[0].ToInt();
                        var compareOperatorType = TypesUtil.FromDescription<CompareOperatorType>(parameters[0].ExtractKorean());
                        if (!compareOperatorType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        switch (compareOperatorType.Value)
                        {
                            case CompareOperatorType.MORE_THAN:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack >= receiveDamage)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.LESS_THAN:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack <= receiveDamage)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.OVER:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack > receiveDamage)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.UNDER:
                                {
                                    if (player.GameState.CurrentPlayer.PlusAttack < receiveDamage)
                                        return true;
                                }
                                return false;
                            default:
                                return false;
                        }
                    }
                case ConditionType.BUFF:
                    {
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);
                        if (!buffType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }
                        return player.HasBuff(buffType.Value);
                    }
                case ConditionType.ALLY_BUFF:
                    {
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);
                        if (!buffType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        foreach (var allyPlayer in player.Team.AlivePlayers)
                        {
                            if (allyPlayer.HasBuff(buffType.Value))
                                return true;
                        }
                    }
                    return false;
                case ConditionType.ATTACKER_CLASS_TYPE:
                    {
                        ClassType? classType = TypesUtil.FromDescription<ClassType>(parameters[0]);
                        if (!classType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        if (player.GameState.CurrentPlayer.TeamType == player.TeamType &&
                            player.GameState.CurrentPlayer.PlayerData.Type == classType.Value)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.KILL_BY_ACTIVE_SKILL:
                    {
                        if (timingType == TimingType.KILL_BY_ACTIVE_SKILL)
                        {
                            return true;
                        }
                    }
                    return false;
                case ConditionType.ENEMY_BUFF:
                    {
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);
                        if (!buffType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        foreach (var enemyPlayer in player.GameState.EnemyTeam.AlivePlayers)
                        {
                            if (enemyPlayer.HasBuff(buffType.Value))
                                return true;
                        }
                    }
                    return false;
                case ConditionType.HP_LESS:
                    {
                        int hp = parameters[0].ToInt();
                        if (player.GetHp() >= hp)
                            return true;
                    }
                    return false;
                case ConditionType.HP_RATIO:
                    {
                        double hp = parameters[0].ToDouble() / 100;
                        var compareOperatorType = TypesUtil.FromDescription<CompareOperatorType>(parameters[0].ExtractKorean());
                        if (!compareOperatorType.HasValue)
                        {
                            InvalidParameter(conditionType, parameters, timingType, player);
                            return false;
                        }

                        switch (compareOperatorType.Value)
                        {
                            case CompareOperatorType.MORE_THAN:
                                {
                                    if (player.GameState.CurrentPlayer.HpRatio >= hp)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.LESS_THAN:
                                {
                                    if (player.GameState.CurrentPlayer.HpRatio <= hp)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.OVER:
                                {
                                    if (player.GameState.CurrentPlayer.HpRatio > hp)
                                        return true;
                                }
                                return false;
                            case CompareOperatorType.UNDER:
                                {
                                    if (player.GameState.CurrentPlayer.HpRatio < hp)
                                        return true;
                                }
                                return false;
                            default:
                                return false;
                        }
                    }
                case ConditionType.AFTER_KILL:
                    {
                        return timingType == TimingType.DEATH_AFTER;
                    }
                case ConditionType.PROBABILITY:
                    {
                        int probability = parameters[0].ToInt();
                        return player.RandomUtil.Get(100) <= probability;
                    }
                case ConditionType.DEADLY_ATTACK:
                    {
                        return timingType == TimingType.DEADABLE_ATTACK;
                    }
                default:
                    LogUtil.WriteError("Not Implemented Condition. {0}", conditionType);
                    return false;
            }
        }
    }
}