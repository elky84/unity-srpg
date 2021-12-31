using Logic.Controller;
using Logic.Entity;
using Logic.Protocols;

using Logic.Types;
using Logic.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Logic.TypesBehaviour
{
    public static class EffectTypeBehaviour
    {
        public static void InvalidParameter(EffectType effectType, List<string> parameters, Entity.Player player)
        {
            LogUtil.WriteError("[Invalid Parameter] effectType:{0}, player:{1}, parameters:{2}", effectType, player.ToLog(), string.Join(",", parameters));
        }

        public static bool OnEffect(this EffectType effectType, List<string> parameters, Entity.Skill skill, Entity.Player player)
        {
            switch (effectType)
            {
                case EffectType.ATTACK_SPEED_INCREASE:
                    {
                        double value = parameters[0].ToDouble() / 100;
                        player.Speed += Math.Min((int)(player.Speed * value), 0);
                        return true;
                    }
                case EffectType.ATTACK_SPEED_DECREASE:
                    {
                        double value = parameters[0].ToDouble() / 100;
                        player.Speed -= Math.Min((int)(player.Speed * value), 0);
                        return true;
                    }
                case EffectType.ATTACK_INCREASE:
                    {
                        int value = parameters[0].ToInt();
                        player.AddAttackRatio = (value / 100);
                        return true;
                    }
                case EffectType.ATTACK_INCREASE_BY_BUFF_COUNT:
                    {
                        int value = parameters[0].ToInt();
                        player.AddAttackRatio = player.Buffs.Count * (value / 100);
                        return true;
                    }
                case EffectType.BAD_STATUS:
                    {
                        var badStatusType = TypesUtil.FromName<BadStatusType>(parameters[0]);
                        if (!badStatusType.HasValue)
                        {
                            InvalidParameter(effectType, parameters, player);
                            return false;
                        }

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.BadStatusType = badStatusType.Value;
                        }

                        player.GameState.WriteEvent(new BadStatus
                        {
                            PlayerIndex = player.PlayerIndex,
                            TargetPlayers = player.GameState.TargetPlayers.ConvertAll(p => p.PlayerIndex).ToList(),
                            BadStatusType = badStatusType.Value
                        });

                        return true;
                    }
                case EffectType.NORMAL_ATTACK:
                    {
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            player.NormalAttack(targetPlayer);
                        }
                    }
                    return true;
                case EffectType.NORMAL_ATTACK_RATIO:
                    {
                        var ratio = parameters[0].ToDouble() / 100;
                        player.AddAttackRatio = ratio;
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            player.NormalAttack(targetPlayer);
                        }
                    }
                    return true;
                case EffectType.NORMAL_ATTACK_COUNT:
                    {
                        var count = parameters[0].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            for (int n = 0; n < count; ++n)
                            {
                                player.NormalAttack(targetPlayer);
                            }
                        }
                    }
                    return true;
                case EffectType.CREATE_COMBO:
                    {
                        int probability = parameters[0].ToInt();
                        var rand = player.RandomUtil.Get(100);
                        if (probability + player.Team.ComboProbability > rand)
                        {
                            player.CreateCombo();
                            LogUtil.WriteInfo("[콤보생성] {0}, {1}, {2}", player.ToLog(), probability, rand);
                            return true;
                        }
                        else
                        {
                            LogUtil.WriteInfo("[콤보생성_실패] {0}, {1}, {2}", player.ToLog(), probability, rand);
                            return false;
                        }
                    }
                case EffectType.RECOVERY_HP_BY_HP:
                    {
                        int recoveryHp = 0;
                        int recoveryValue1 = parameters[1].ToInt();
                        int recoveryValue2 = parameters[2].ToInt();

                        int value = parameters[0].ToInt();
                        var compareOperatorType = TypesUtil.FromDescription<CompareOperatorType>(parameters[0].ExtractKorean());
                        if (!compareOperatorType.HasValue)
                        {
                            InvalidParameter(effectType, parameters, player);
                            return false;
                        }

                        switch (compareOperatorType.Value)
                        {
                            case CompareOperatorType.MORE_THAN:
                                {
                                    if (player.GetHp() >= value)
                                        recoveryHp = recoveryValue1;
                                    else
                                        recoveryHp = recoveryValue2;
                                }
                                break;
                            case CompareOperatorType.LESS_THAN:
                                {
                                    if (player.GetHp() <= value)
                                        recoveryHp = recoveryValue1;
                                    else
                                        recoveryHp = recoveryValue2;
                                }
                                break;
                            case CompareOperatorType.OVER:
                                {
                                    if (player.GetHp() > value)
                                        recoveryHp = recoveryValue1;
                                    else
                                        recoveryHp = recoveryValue2;
                                }
                                break;
                            case CompareOperatorType.UNDER:
                                {
                                    if (player.GetHp() < value)
                                        recoveryHp = recoveryValue1;
                                    else
                                        recoveryHp = recoveryValue2;
                                }
                                break;
                            default:
                                return false;
                        }

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.Recovery(player, recoveryHp, effectType);
                        }
                        return true;
                    }
                case EffectType.DECREASE_BUFF:
                    {
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);
                        if (!buffType.HasValue)
                        {
                            InvalidParameter(effectType, parameters, player);
                            return false;
                        }

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.Buffs.RemoveAll(x => x.Type == buffType.Value);
                        }
                        return true;
                    }
                case EffectType.DAMAGE:
                    {
                        int dmg = parameters[0].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.Damage(player, dmg, effectType);
                        }
                        return true;
                    }
                case EffectType.DAMAGE_COUNT:
                    {
                        int count = parameters[0].ToInt();
                        for (int n = 0; n < count; ++n)
                        {
                            foreach (var targetPlayer in player.GameState.TargetPlayers)
                            {
                                targetPlayer.Damage(player, player.PlusAttack, effectType);
                            }
                        }
                        return true;
                    }
                case EffectType.DAMAGE_RATIO:
                    {
                        int ratio = parameters[0].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.Damage(player, player.PlusAttack * (ratio / 100), effectType);
                        }
                        return true;
                    }
                case EffectType.DAMAGE_BY_CLASS:
                    {
                        int damageValue1 = parameters[1].ToInt();
                        int damageValue2 = parameters[2].ToInt();
                        var classType = TypesUtil.FromDescription<ClassType>(parameters[0]);

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            int damageByClass = 0;
                            if (classType.Value == player.Type)
                                damageByClass = damageValue1;
                            else
                                damageByClass = damageValue2;

                            targetPlayer.Damage(player, damageByClass, effectType);
                        }
                        return true;
                    }
                case EffectType.DAMAGE_BY_BUFF:
                    {
                        int damageValue1 = parameters[1].ToInt();
                        int damageValue2 = parameters[2].ToInt();
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);

                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            int damageByType = 0;
                            if (targetPlayer.HasBuff(buffType.Value))
                                damageByType = damageValue1;
                            else
                                damageByType = damageValue2;

                            targetPlayer.Damage(player, damageByType, effectType);
                        }
                        return true;
                    }
                case EffectType.DAMAGE_BY_HP:
                    {
                        int hp = parameters[0].ToInt();
                        var compareOperatorType = TypesUtil.FromDescription<CompareOperatorType>(parameters[0].ExtractKorean());
                        if (!compareOperatorType.HasValue)
                        {
                            InvalidParameter(effectType, parameters, player);
                            return false;
                        }

                        int damageValue1 = parameters[1].ToInt();
                        int damageValue2 = parameters[2].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            int damageValue = 0;

                            switch (compareOperatorType.Value)
                            {
                                case CompareOperatorType.MORE_THAN:
                                    {
                                        if (targetPlayer.GetHp() >= hp)
                                            damageValue = damageValue1;
                                        else
                                            damageValue = damageValue2;
                                    }
                                    break;
                                case CompareOperatorType.LESS_THAN:
                                    {
                                        if (targetPlayer.GetHp() <= hp)
                                            damageValue = damageValue1;
                                        else
                                            damageValue = damageValue2;
                                    }
                                    break;
                                case CompareOperatorType.OVER:
                                    {
                                        if (targetPlayer.GetHp() > hp)
                                            damageValue = damageValue1;
                                        else
                                            damageValue = damageValue2;
                                    }
                                    break;
                                case CompareOperatorType.UNDER:
                                    {
                                        if (targetPlayer.GetHp() < hp)
                                            damageValue = damageValue1;
                                        else
                                            damageValue = damageValue2;
                                    }
                                    break;
                                default:
                                    LogUtil.WriteError("Invalid CompareOperatorType:{0}, EffectType:{1}, Parameters:{2}", compareOperatorType, effectType, string.Join(",", parameters));
                                    break;
                            }

                            targetPlayer.Damage(player, damageValue, effectType);
                        }
                        return true;
                    }
                case EffectType.SKILL_COOLTIME_DECREASE:
                    {
                        var targetType = TypesUtil.FromDescription<TargetType>(parameters[0]);
                        int cooltime = parameters[1].ToInt();

                        foreach (var targetPlayer in player.GameState.GetTargets(player, targetType.Value, null, out bool needSelection))
                        {
                            foreach (var activeSkill in targetPlayer.ActiveSkill)
                            {
                                activeSkill.Cooltime -= cooltime;
                            }

                            foreach (var passiveSkill in targetPlayer.PassiveSkill)
                            {
                                passiveSkill.Cooltime -= cooltime;
                            }

                        }
                        return true;
                    }
                case EffectType.SKILL_COOLTIME_INCREASE:
                    {
                        var targetType = TypesUtil.FromDescription<TargetType>(parameters[0]);
                        int cooltime = parameters[1].ToInt();

                        foreach (var targetPlayer in player.GameState.GetTargets(player, targetType.Value, null, out bool needSelection))
                        {
                            foreach (var activeSkill in targetPlayer.ActiveSkill)
                            {
                                activeSkill.Cooltime += cooltime;
                            }

                            foreach (var passiveSkill in targetPlayer.PassiveSkill)
                            {
                                passiveSkill.Cooltime += cooltime;
                            }

                        }
                        return true;
                    }
                case EffectType.ACTIVE_COOLTIME_DECREASE:
                    {
                        int cooltime = parameters[0].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            foreach (var activeSkill in targetPlayer.ActiveSkill)
                            {
                                activeSkill.Cooltime -= cooltime;
                            }

                        }
                        return true;
                    }
                case EffectType.ACTIVE_COOLTIME_INCREASE:
                    {
                        int cooltime = parameters[0].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            foreach (var activeSkill in targetPlayer.ActiveSkill)
                            {
                                activeSkill.Cooltime += cooltime;
                            }

                        }
                        return true;
                    }
                case EffectType.COMBO_COOLTIME_DECREASE:
                    {
                        int cooltime = int.Parse(parameters[0]);
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            foreach (var passiveSkill in targetPlayer.PassiveSkill.Where(x => x.Conditions.Where(cond => cond.ConditionType == ConditionType.COMBO_ATTACK).Any()))
                            {
                                passiveSkill.Cooltime -= cooltime;
                            }
                        }
                        return true;
                    }
                case EffectType.DAMAGE_BY_EXIST:
                    {
                        //TODO param이 죽음인지, 아닌지 등의 다양한 상태를 담게끔 수정해야 함.
                        string param = parameters[0];
                        int damageValue1 = parameters[1].ToInt();
                        int damageValue2 = parameters[2].ToInt();

                        bool existDeath = false;
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            if (targetPlayer.IsDead())
                            {
                                existDeath = true;
                                break;
                            }
                        }

                        int damageByExist = existDeath ? damageValue1 : damageValue2;
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.Damage(player, damageByExist, effectType);
                        }
                        return true;
                    }
                case EffectType.INSTEAD:
                    {
                        // 리스트지만, 일반 공격 대상으로 한명만 있을 것을 가정. 차지 스킬이나, 액티브 스킬로 들어와서 여러명이 타겟일 경우 너무 많이 대신 맞아주므로 로직을 개선할 필요가 있거나, OP 효과가 될 듯.
                        // 아군이 타겟으로 지정됐다는 가정도 존재하는 코드임.
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            player.Damage((Entity.Player)player.GameState.CurrentPlayer, (double)player.GameState.CurrentPlayer.Damage, effectType);
                        }
                        player.GameState.CurrentPlayer.DecreaseDamage = player.GameState.CurrentPlayer.Damage;
                        return true;
                    }
                case EffectType.COMBO_PROBABILITY:
                    {
                        int probability = parameters[0].ToInt();
                        player.Team.ComboProbability += probability;
                        return true;
                    }
                case EffectType.DAMAGE_ALLEVIATE:
                    {
                        double value = parameters[0].ToInt() / 100;
                        player.GameState.CurrentPlayer.DecreaseDamage = player.GameState.CurrentPlayer.Damage * value;
                        return true;
                    }
                case EffectType.ADDITIONAL_ATTACK:
                    {
                        // 일반 공격으로 한번 더 때린다고 해석해서 구현해놓음. 데미지만 추가일 경우 수정 필요.
                        player.Attack((List<Entity.Player>)player.GameState.TargetPlayers, player.PlusAttack, TimingType.ACTIVE_SKILL_AFTER);
                        return true;
                    }
                case EffectType.BUFF:
                    {
                        var buffType = TypesUtil.FromName<BuffType>(parameters[0]);
                        if (!buffType.HasValue)
                        {
                            InvalidParameter(effectType, parameters, player);
                            return false;
                        }

                        int cooltime = parameters.Count >= 2 ? parameters[1].ToInt() : 1;
                        int probability = parameters.Count >= 3 ? parameters[2].ToInt() : 100;
                        int buffValue = parameters.Count >= 4 ? parameters[3].ToInt() : 0;

                        if (probability > player.RandomUtil.Get(100))
                        {
                            player.Team.AddBuff((List<Entity.Player>)player.GameState.TargetPlayers, buffType.Value, cooltime, buffValue);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                case EffectType.RECOVERY_HP:
                    {
                        int value = parameters[0].ToInt();
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.Recovery(player, value, effectType);
                        }
                    }
                    return true;
                case EffectType.DAMAGE_BY_ORDER:
                    {
                        var damages = parameters.Select(int.Parse).ToList();
                        for (int index = 0; index < damages.Count; ++index)
                        {
                            if (index >= player.GameState.TargetPlayers.Count)
                            {
                                break;
                            }

                            player.GameState.TargetPlayers[index].Damage(player, damages[index], effectType);
                        }
                    }
                    return true;
                case EffectType.CHANGE_ORDER:
                    {
                        player.GameState.InvertSortType = true;
                    }
                    return true;
                case EffectType.REMOVE_ALL_BUFF:
                    {
                        player.RemoveAllBuff();
                    }
                    return true;
                case EffectType.COMBO_DAMAGE:
                    {
                        foreach (var targetPlayer in player.GameState.TargetPlayers)
                        {
                            targetPlayer.ComboAttack(player);
                        }
                        return true;
                    }
                default:
                    LogUtil.WriteError("Not Implemented Effect. {0}", effectType);
                    return false;
            }
        }
    }
}
