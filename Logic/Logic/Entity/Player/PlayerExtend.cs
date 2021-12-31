using Logic.Controller;
using Logic.MasterData;
using Logic.Exceptions;
using Logic.Protocols;

using Logic.Types;
using Logic.Util;
using System;
using System.Collections.Generic;

namespace Logic.Entity
{
    public static class PlayerExtend
    {
        public static bool IsDead(this Player player)
        {
            return player.GetHp() <= 0;
        }

        public static void Recovery(this Player player, Player activePlayer, double quantity, EffectType effectType)
        {
            player.AddHp(activePlayer, quantity, effectType);
        }

        public static void ComboAttack(this Player player, Player attacker)
        {
            player.GameState.WriteEvent(new ComboAttack { TargetPlayers = new List<int> { player.PlayerIndex }, PlayerIndex = attacker.PlayerIndex, Count = player.GameState.ComboCount });
            player.Damage(attacker, (double)(attacker.PlusAttack + (attacker.PlusAttack * (attacker.GameState.ComboCount * 0.2))), EffectType.COMBO_DAMAGE);
        }

        public static void Damage(this Player player, Player attacker, double damage, EffectType effectType)
        {
            // 데미지를 받을 때 확인. 콤보 공격의 경우, 한 타겟을 지정해서 여러번 때리기 때문에, 죽은 놈이 또 죽을 수 있음.
            if (player.IsDead())
            {
                return;
            }

            LogUtil.WriteDamage("[데미지받음] attacker:{0}, target:{1}, damaged:{2}, Hp:{3} => {4}", attacker.ToLog(), player.ToLog(), damage, player.GetHp(), player.GetHp() - damage);

            player.AddHp(attacker, -damage, effectType);

            if (player.IsDead())
            {
                player.Team.AlivePlayers.Remove(player);

                player.GameState.WriteEvent(new Dead { PlayerIndex = player.PlayerIndex }, false);

                LogUtil.WriteDead("[죽음] attacker:{0}, target:{1}, damaged:{2}", attacker.ToLog(), player.ToLog(), damage);

                if (player.GameState.IsGameEnd())
                {
                    throw new GameEndException { TeamType = player.GameState.GetVictoryTeam().TeamType };
                }
            }
        }

        public static Player Setup(this Player player, Team team)
        {
            player.SetHp(player.PlayerData.Hp);

            team.Players.Add(player);

            SkillExtend.AddSkill(player, player.PlayerData.Skill1, 1);
            SkillExtend.AddSkill(player, player.PlayerData.Skill2, 2);
            SkillExtend.AddSkill(player, player.PlayerData.Skill3, 3);
            SkillExtend.AddSkill(player, player.PlayerData.Skill4, 4);
            return player;
        }

        public static List<int> ToPlayerIndexes(this List<Player> players)
        {
            return players.ConvertAll(player => player.PlayerIndex);
        }

        public static void Attack(this Player player, List<Player> destPlayers, Double damage, TimingType timingType)
        {
            player.GameState.WriteEvent(new NormalAttack { PlayerIndex = player.PlayerIndex, TargetPlayers = destPlayers.ToPlayerIndexes(), Critical = player.CriticalAttack });

            foreach (var destPlayer in destPlayers)
            {
                if (destPlayer.IsDead())
                {
                    continue;
                }

                if (player.TeamType == destPlayer.TeamType)
                {
                    LogUtil.WriteError("Attack Same Team, attacker:{0}, target:{1}, damaged:{2}, timingType:{3}",
                        player.ToLog(), destPlayer.ToLog(), damage, timingType);
                }

                if (destPlayer.PlayerData.Defense > destPlayer.RandomUtil.Get(100))
                {
                    player.GameState.WriteEvent(new Guard
                    {
                        PlayerIndex = destPlayer.PlayerIndex,
                        AttackerPlayerIndex = player.PlayerIndex
                    });

                    LogUtil.WriteInfo("[막기성공] Same Team, attacker:{0}, target:{1}, damaged:{2}, timingType:{3}, DefenseRate:{4}",
                        player.ToLog(), destPlayer.ToLog(), damage, timingType, destPlayer.PlayerData.Defense);
                    continue;
                }

                damage = damage - destPlayer.Defense;
                if (damage <= 0)
                {
                    continue;
                }

                if (destPlayer.GetHp() <= damage)
                {
                    player.GameState.RunPassiveSkills(TimingType.DEADABLE_ATTACK);
                }

                destPlayer.Damage(player, damage, EffectType.NORMAL_ATTACK);

                if (destPlayer.CheckCounterAttack())
                {
                    destPlayer.CounterAttack(player);
                }
            }
        }

        public static void CounterAttack(this Player player, Player destPlayer)
        {
            player.GameState.RunPassiveSkills(TimingType.COUNTER_ATTACK);

            // TODO 크리 발동 시점이 이거 한번인게 맞는가?
            player.CheckCritical();

            // 대신 맞기 등으로 데미지가 경감되어서 0이하가 들어옴.
            if (player.Damage <= 0)
            {
                return;
            }

            player.Attack(new List<Player> { destPlayer }, player.Damage, TimingType.COUNTER_ATTACK);
        }


        public static void NormalAttack(this Player player, Player destPlayer)
        {
            player.GameState.RunPassiveSkills(TimingType.NORMAL_ATTACK_BEFORE);

            // TODO 크리 발동 시점이 이거 한번인게 맞는가?
            player.CheckCritical();

            // 대신 맞기 등으로 데미지가 경감되어서 0이하가 들어옴.
            if (player.Damage <= 0)
            {
                return;
            }

            player.Attack(new List<Player> { destPlayer }, player.Damage, TimingType.NORMAL_ATTACK_AFTER);

            player.GameState.RunPassiveSkills(TimingType.NORMAL_ATTACK_AFTER);
        }

        public static bool Move(this Player player, PlayerPosition playerPosition)
        {
            var diff = player.PlayerPosition.Diff(playerPosition);
            if (diff > player.PlayerData.Move)
            {
                return false;
            }

            if (diff <= 0)
            {
                return false;
            }

            LogUtil.WriteDamage("[이동] 플레이어:{0} 기존위치:{1},{2}, 이동위치:{3},{4}", player.Name, player.PlayerPosition.X, player.PlayerPosition.Y, playerPosition.X, playerPosition.Y);
            player.PlayerPosition = playerPosition;

            player.GameState.WriteEvent(new PositionMove
            {
                PlayerIndex = player.PlayerIndex,
                PlayerPosition = player.PlayerPosition.ToProtocol()
            });
            return true;
        }

        public static void CreateCombo(this Player player)
        {
            player.Team.ComboProbability = 0;
            ++player.GameState.ComboCount;

            player.GameState.RunPassiveSkills(TimingType.COMBO_ATTACK);
        }

        public static bool ActiveSkill(this Player player, Skill skill)
        {
            return skill.OnEffect(player);
        }

        public static void DeathAlly(this Player player)
        {
            player.GameState.RunPassiveSkills(TimingType.DEATH_AFTER);
        }

        public static void DeathEnemy(this Player player)
        {
            player.GameState.RunPassiveSkills(TimingType.DEATH_AFTER);
        }
    }
}
