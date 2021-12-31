using Logic.Controller;
using Logic.MasterData;
using Logic.Protocols;

using Logic.Types;
using Logic.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static Logic.Constants.LogicConstants;

namespace Logic.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Team
    {
        [JsonProperty]
        public TeamType TeamType { get; set; }

        [JsonProperty]
        public List<Player> Players { get; } = new List<Player>();

        [JsonProperty]
        public int ComboProbability { get; set; }

        public RandomUtil RandomUtil { get; } = new RandomUtil();

        public GameState GameState { get; set; }

        public List<Player> AlivePlayers => Players.Where(player => !player.IsDead()).ToList();

        public Team()
        {
        }

        public Team(TeamType teamType)
        {
            this.TeamType = teamType;
        }

        public Player AliveRandom => AlivePlayers[RandomUtil.Get(AlivePlayers.Count)];

        public Player AttackableRandom()
        {
            var targets = AlivePlayers.Where(x => !x.HasBuff(BuffType.STUN)).ToArray();
            if (targets.Length <= 0)
            {
                return null;
            }

            return targets[RandomUtil.Get(targets.Length)];
        }

        public Player ActiveSkillUsableRandom()
        {
            var targets = AlivePlayers.Where(x => !x.HasBuff(BuffType.STUN) && !x.isActiveSkillCooltime()).ToArray();
            if (targets.Length <= 0)
            {
                return null;
            }

            return targets[RandomUtil.Get(targets.Length)];
        }

        public Player TargetRandom()
        {
            var targets = AlivePlayers.Where(x => x.HasBuff(BuffType.TAUNT)).ToArray();
            if (targets.Any())
            {
                return targets[RandomUtil.Get(targets.Length)];
            }
            else
            {
                targets = AlivePlayers.Where(x => !x.HasBuff(BuffType.HIDDEN)).ToArray();
                if (targets.Any())
                    return targets[RandomUtil.Get(targets.Length)];
                else
                    return null;
            }
        }

        public List<Player> PlayablePlayers => AlivePlayers.Where(x => !x.HasBuff(BuffType.STUN)).ToList();

        private void ApplyCoolTime(List<Skill> skills, int cooltime)
        {
            foreach (var skill in skills)
            {
                if (skill.CooltimeConfig != 0)
                {
                    skill.Cooltime += cooltime;
                }
            }
        }

        public void ActiveSkillCoolTime(List<Player> targets, int cooltime)
        {
            foreach (var player in AlivePlayers)
            {
                ApplyCoolTime(player.ActiveSkill, cooltime);
            }
        }

        public void PassiveSkillCoolTime(List<Player> targets, int cooltime)
        {
            foreach (var player in AlivePlayers)
            {
                ApplyCoolTime(player.PassiveSkill, cooltime);

            }
        }

        public void AddBuff(List<Player> targets, BuffType type, int cooltime, int value = 0)
        {
            var buff = new Buff { Type = type, RemainCooltime = cooltime, Value = value };

            GameState.WriteEvent(new Logic.Protocols.BuffEffect { TargetPlayers = targets.ToPlayerIndexes(), Buff = buff.ToProtocol() }, false);

            foreach (var player in targets)
            {
                player.Buffs.Add(buff);
            }
        }

        public string ToPlayerLog()
        {
            return string.Join(",", Players.ToDictionary(player => player.Id.ToString()).Keys.ToArray());
        }
    }
}
