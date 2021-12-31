using Logic.Controller;
using Logic.MasterData;
using Logic.Protocols;

using Logic.Types;
using Logic.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Player
    {
        public GameState GameState { get; set; }

        [JsonProperty]
        public PlayerData PlayerData { get; set; } = new PlayerData();

        public Team Team { get; set; }

        [JsonProperty]
        public int PlayerIndex { get; set; }

        [JsonProperty]
        public TeamType TeamType { get; set; }

        [JsonProperty]
        protected double Hp { get; set; }

        [JsonProperty]
        public List<Buff> Buffs { get; set; } = new List<Buff>();

        [JsonProperty]
        public List<Skill> PassiveSkill { get; set; } = new List<Skill>();

        [JsonProperty]
        public List<Skill> ActiveSkill { get; set; } = new List<Skill>();

        [JsonProperty]
        public BadStatusType BadStatusType { get; set; }

        [JsonProperty]
        public PlayerPosition PlayerPosition { get; set; } = new PlayerPosition();

        [JsonProperty]
        public int Speed { get; set; }

        public double Damage => Math.Max(PlusAttack - DecreaseDamage, 0);

        public double DecreaseDamage { get; set; }

        public RandomUtil RandomUtil { get; } = new RandomUtil();


        public int Id
        {
            get
            {
                return PlayerData.Id;
            }
            set
            {
                PlayerData.Id = value;
            }
        }

        public double Attack => PlayerData.Attack;

        public double Defense => PlayerData.Defense;

        public double AddAttackRatio { get; set; }

        public double HpRatio => Hp / PlayerData.Hp;

        public double PlusAttack
        {
            get
            {
                var attackBuff = Buffs.Where(x => x.RemainCooltime > 0 && x.Type == BuffType.ATTACK_UP);
                if (attackBuff.Any())
                {
                    var plusAttack = Attack;
                    foreach (var buff in attackBuff)
                    {
                        plusAttack += buff.Value;
                    }
                    return (CriticalAttack ? 1.3 : 1) * plusAttack + (plusAttack * AddAttackRatio);
                }
                else
                {
                    return (CriticalAttack ? 1.3 : 1) * Attack + (Attack * AddAttackRatio);
                }
            }
        }

        public bool CriticalAttack { get; set; }

        public String Name => PlayerData.Name;

        public ClassType Type => PlayerData.Type;

        public double GetHp()
        {
            return Hp;
        }

        public void SetHp(double hp)
        {
            Hp = hp;
        }

        public bool CheckCounterAttack()
        {
            return PlayerData.Counter > RandomUtil.Get(100);
        }


        public void CheckCritical()
        {
            CriticalAttack = PlayerData.Critical > RandomUtil.Get(100);
        }

        public void AddHp(Player player, double value, EffectType effectType)
        {
            Hp += value;

            GameState.WriteEvent(new HpChange { PlayerIndex = player.PlayerIndex, DestPlayerIndex = this.PlayerIndex, Value = -value, EffectType = effectType }, false);
        }


        public bool isActiveSkillCooltime()
        {
            foreach (var skill in ActiveSkill)
            {
                if (skill.Cooltime > 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasBuff(BuffType type)
        {
            return Buffs.Where(x => x.Type == type).Any();
        }

        public void RemoveAllBuff()
        {
            foreach (var buffPlayer in Buffs)
            {
                GameState.WriteEvent(new BuffEffectRemove { Buff = buffPlayer.ToProtocol(), PlayerIndex = PlayerIndex });
            }

            Buffs.Clear();
        }

        public string ToLog()
        {
            return string.Format("[Id:{0}, Name:{1}, Hp:{2}, Speed:{3}, Position:{4},{5}]", this.Id, this.Name, this.Hp, this.Speed, this.PlayerPosition.X, this.PlayerPosition.Y);
        }

        public List<Buff> GetBuffs(BuffType type)
        {
            return Buffs.Where(x => x.Type == type).ToList();
        }

        public void DecreaseBuffCooltime(BuffType type, int cooltime)
        {
            foreach (var buffPlayer in Buffs.Where(x => x.Type == type || type == BuffType.ALL).ToList())
            {
                buffPlayer.RemainCooltime -= cooltime;
                if (buffPlayer.RemainCooltime <= 0)
                {
                    GameState.WriteEvent(new BuffEffectRemove { Buff = buffPlayer.ToProtocol(), PlayerIndex = PlayerIndex });
                    Buffs.Remove(buffPlayer);
                }
            }
        }

        public void DecreaseSkillCooltime(int cooltime)
        {
            foreach (var skill in ActiveSkill)
            {
                if (skill.Cooltime > 0)
                {
                    skill.Cooltime -= cooltime;
                }
            }

            foreach (var skill in PassiveSkill)
            {
                if (skill.Cooltime > 0)
                {
                    skill.Cooltime -= cooltime;
                }
            }
        }

        public Skill GetSkill(int skillId)
        {
            foreach (var skill in ActiveSkill)
            {
                if (skill.SkillId == skillId)
                {
                    return skill;
                }
            }
            return null;
        }

        public List<Skill> GetUsableSkill()
        {
            return ActiveSkill.Where(skill => skill.Cooltime <= 0).ToList();
        }

        public Skill RandomSKill()
        {
            var usableSkill = GetUsableSkill();
            if (usableSkill.Count <= 0)
            {
                return null;
            }

            return usableSkill[RandomUtil.Get(usableSkill.Count)];
        }

        public Protocols.Player ToProtocol()
        {
            return new Protocols.Player
            {
                PlayerIndex = PlayerIndex,
                TeamType = TeamType,
                Hp = GetHp(),
                Id = Id,
                Attack = Attack,
                Speed = Speed,
                ActiveSkill = ActiveSkill.ConvertAll(skill => skill.ToProtocol()),
                PassiveSkill = PassiveSkill.ConvertAll(skill => skill.ToProtocol()),
                PlayerPosition = PlayerPosition.ToProtocol()
            };
        }
    }
}
