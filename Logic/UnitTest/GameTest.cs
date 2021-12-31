using Logic.Common;
using Logic.Controller;
using Logic.MasterData;
using Logic.Entity;

using Logic.Types;
using Logic.Util;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class GameTest
    {
        private List<TeamSetting> TeamSettings { get; set; }

        private List<PlayerData> PlayerDatas { get; set; }

        private HashSet<TargetType> TargetTypes = new HashSet<TargetType>();

        private HashSet<ConditionType> ConditionTypes = new HashSet<ConditionType>();

        private HashSet<EffectType> EffectTypes = new HashSet<EffectType>();

        public class TestGameController : GameController
        {

            public void AddTeam(Team team)
            {
                Teams.Add(team);
            }
        }

        public class TestPlayer : Player
        {
            public void SetupHp(double hp)
            {
                Hp = hp;
            }
        }

        private TestGameController CreateGameController(int bluePlayerCount = 1, int redPlayerCount = 1)
        {
            var controller = new TestGameController();
            var blueTeam = new Team
            {
                TeamType = TeamType.BLUE,
                GameState = controller
            };

            controller.AddTeam(blueTeam);

            var redTeam = new Team
            {
                TeamType = TeamType.RED,
                GameState = controller
            };

            controller.AddTeam(redTeam);

            for (int n = 0; n < bluePlayerCount; ++n)
            {
                var playerData = PlayerDatas[n % PlayerDatas.Count];
                var player = new TestPlayer
                {
                    GameState = controller,
                    PlayerData = playerData,
                    Team = blueTeam,
                    TeamType = blueTeam.TeamType
                };
                player.SetupHp(playerData.Hp);

                if (controller.CurrentPlayer == null)
                {
                    controller.CurrentPlayer = player;
                }

                blueTeam.Players.Add(player);
            }

            for (int n = 0; n < redPlayerCount; ++n)
            {
                var playerData = PlayerDatas[(blueTeam.Players.Count + n) % PlayerDatas.Count];
                var target = new TestPlayer
                {
                    GameState = controller,
                    PlayerData = playerData,
                    Team = redTeam,
                    TeamType = redTeam.TeamType
                };

                target.SetupHp(playerData.Hp);

                redTeam.Players.Add(target);

                if (controller.TargetPlayers.Count <= 0)
                {
                    controller.TargetPlayers.Add(target);
                }
            }

            return controller;
        }

        [SetUp]
        public void Setup()
        {
            string teamSettingJson = System.IO.File.ReadAllText(@"Json\TeamSetting.json");
            TeamSettings = JsonConvert.DeserializeObject<List<TeamSetting>>(teamSettingJson);

            string playerDataJson = System.IO.File.ReadAllText(@"Json\PlayerData.json");
            PlayerDatas = JsonConvert.DeserializeObject<List<PlayerData>>(playerDataJson);
        }

        [Test, Order(1)]
        public void TestSkillTarget()
        {
            {
                var gameController = CreateGameController(4, 4);

                CheckSkillTarget(gameController, TargetType.SELF, new List<string> { }, new List<Player> { gameController.CurrentPlayer });

            }

            {
                var gameController = CreateGameController(4, 4);
                CheckSkillTargetCount(gameController, TargetType.ALLY, new List<string> { 3.ToString() }, 3);
            }

            {
                var gameController = CreateGameController(4, 4);
                CheckSkillTargetCount(gameController, TargetType.ENEMY, new List<string> { 2.ToString() }, 2);
            }

            {
                var gameController = CreateGameController(4, 4);

                CheckSkillTarget(gameController, TargetType.ALLY_ALL, new List<string> { ClassType.ATTACK.GetDescription() }, gameController.CurrentTeam.AlivePlayers);

            }

            {
                var gameController = CreateGameController(4, 4);

                CheckSkillTarget(gameController, TargetType.ENEMY_ALL, new List<string> { }, gameController.EnemyTeam.AlivePlayers);

            }

            {
                var gameController = CreateGameController(4, 4);

                CheckSkillTarget(gameController, TargetType.ALLY_EVERYONE, new List<string> { }, gameController.CurrentTeam.AlivePlayers);

            }

            {
                var gameController = CreateGameController(4, 4);
                Assert.AreEqual(GetSkillTargets(gameController, TargetType.ENEMY_EVERYONE, new List<string> { }), gameController.EnemyTeam.AlivePlayers);

                CheckSkillTarget(gameController, TargetType.ENEMY_EVERYONE, new List<string> { }, gameController.EnemyTeam.AlivePlayers);

            }

            {
                var gameController = CreateGameController(4, 4);
                CheckSkillTargetCount(gameController, TargetType.ALLY_SELECT, new List<string> { }, gameController.EnemyTeam.AlivePlayers.Count);

            }


            {
                var gameController = CreateGameController(4, 4);

                CheckSkillTargetCount(gameController, TargetType.ENEMY_SELECT, new List<string> { ClassType.ATTACK.GetDescription() }, gameController.EnemyTeam.AlivePlayers.Count);
            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.CurrentTeam.Players);
                players.Remove(gameController.CurrentPlayer);

                CheckSkillTarget(gameController, TargetType.ALLY_ALL_EXCEPT_SELF, new List<string> { }, players);

            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.EnemyTeam.Players).OrderBy(x => x.GetHp()).ToList();

                CheckSkillTargetHp(gameController, TargetType.LOWHP_ENEMY, new List<string> { }, players);

            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.EnemyTeam.Players).OrderByDescending(x => x.GetHp()).ToList();

                CheckSkillTargetHp(gameController, TargetType.HIGHHP_ENEMY, new List<string> { }, players);

            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.EnemyTeam.Players).OrderByDescending(x => x.GetHp());

                CheckSkillTarget(gameController, TargetType.ENEMY_ORDER_BY_HP, new List<string> { SortType.DESCENDING.GetDescription() }, players);

            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.EnemyTeam.Players);
                players.RemoveAll(player => player.Type != ClassType.ATTACK);

                CheckSkillTarget(gameController, TargetType.CLASS_ENEMY, new List<string> { ClassType.ATTACK.GetDescription() }, players);

            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.CurrentTeam.Players);
                players.RemoveAll(player => player.Type != ClassType.ATTACK);

                CheckSkillTarget(gameController, TargetType.CLASS_ALLY, new List<string> { ClassType.ATTACK.GetDescription() }, players);
            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.EnemyTeam.Players);
                players.RemoveAll(player => player.Type == ClassType.ATTACK);

                CheckSkillTarget(gameController, TargetType.WITHOUT_CLASS_ENEMY, new List<string> { ClassType.ATTACK.GetDescription() }, players);

            }

            {
                var gameController = CreateGameController(4, 4);
                var players = new List<Player>(gameController.CurrentTeam.Players);
                players.RemoveAll(player => player.Type == ClassType.ATTACK);

                CheckSkillTarget(gameController, TargetType.WITHOUT_CLASS_ALLY, new List<string> { ClassType.ATTACK.GetDescription() }, players);
            }

            {
                var gameController = CreateGameController(4, 4);
                CheckSkillTarget(gameController, TargetType.ANY, new List<string> { }, gameController.ActivePlayers());
            }

            {
                var gameController = CreateGameController(4, 4);
                CheckSkillTarget(gameController, TargetType.ALLY_BY_BUFF, new List<string> { BuffType.BURN.GetDescription() }, new List<Player> { });
            }

            {
                var gameController = CreateGameController(4, 4);
                CheckSkillTarget(gameController, TargetType.ENEMY_BY_BUFF, new List<string> { BuffType.BURN.GetDescription() }, new List<Player> { });
            }
        }

        private void CheckSkillTargetCount(GameController gameController, TargetType targetType, List<string> parameters, int expectCount)
        {
            Assert.AreEqual(GetSkillTargets(gameController, targetType, parameters).Count, expectCount);
            TargetTypes.Add(targetType);
        }

        private void CheckSkillTarget(GameController gameController, TargetType targetType, List<string> parameters, object obj)
        {
            Assert.AreEqual(GetSkillTargets(gameController, targetType, parameters), obj);
            TargetTypes.Add(targetType);
        }

        private void CheckSkillTargetHp(GameController gameController, TargetType targetType, List<string> parameters, List<Player> players)
        {
            Assert.AreEqual(GetSkillTargets(gameController, targetType, parameters)[0].GetHp(), players[0].GetHp());
            TargetTypes.Add(targetType);
        }


        private List<Player> GetSkillTargets(GameController gameController, TargetType targetType, List<string> parameters)
        {
            var skill = new Skill
            {
                Target = new Logic.Entity.SkillTarget { TargetType = targetType, Targets = parameters }
            };

            gameController.CurrentPlayer.PassiveSkill.Add(skill);
            return gameController.GetTargets(gameController.CurrentPlayer, targetType, parameters, out var needSelection);
        }

        [Test, Order(2)]
        public void TestSkillCondition()
        {
            CheckSkillCondition(ConditionType.COUNTER_ATTACK, new List<string> { }, TimingType.COUNTER_ATTACK);

            {
                var controller = CreateGameController();
                controller.CurrentPlayer.BadStatusType = BadStatusType.AIR;
                CheckSkillCondition(ConditionType.BAD_STATUS, new List<string> { BadStatusType.AIR.ToString() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }


            {
                var controller = CreateGameController();
                controller.CurrentPlayer.SetHp(20);
                CheckSkillCondition(ConditionType.HP_RATIO, new List<string> { 20.ToString() + CompareOperatorType.LESS_THAN.GetDescription() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            {
                var controller = CreateGameController();
                controller.CurrentPlayer.SetHp(20);

                CheckSkillCondition(ConditionType.DAMAGE_BY_HP_RATIO, new List<string> { 10.ToString() + CompareOperatorType.LESS_THAN.GetDescription() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            CheckSkillCondition(ConditionType.DAMAGE, new List<string> { 5.ToString() + CompareOperatorType.OVER.GetDescription() });

            {
                var controller = CreateGameController();
                controller.CurrentPlayer.Team.AddBuff(new List<Player> { controller.CurrentPlayer }, BuffType.STUN, 2);

                CheckSkillCondition(ConditionType.BUFF, new List<string> { BuffType.STUN.ToString() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            {
                var controller = CreateGameController();
                CheckSkillConditionEnemy(ConditionType.ENEMY, new List<string> { }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            {
                var controller = CreateGameController();
                controller.CurrentPlayer.Team.AddBuff(new List<Player> { controller.CurrentPlayer }, BuffType.STUN, 2);
                CheckSkillCondition(ConditionType.ALLY_BUFF, new List<string> { BuffType.STUN.ToString() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            {
                var controller = CreateGameController();
                controller.CurrentPlayer.Team.AddBuff(controller.TargetPlayers, BuffType.STUN, 2);
                CheckSkillCondition(ConditionType.ENEMY_BUFF, new List<string> { BuffType.STUN.ToString() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            CheckSkillCondition(ConditionType.HP_LESS, new List<string> { 5.ToString() });
            CheckSkillCondition(ConditionType.AFTER_KILL, new List<string> { }, TimingType.DEATH_AFTER);
            CheckSkillCondition(ConditionType.NORMAL_ATTACK, new List<string> { TimingParameterType.BEFORE.GetDescription() });
            CheckSkillCondition(ConditionType.PROBABILITY, new List<string> { 100.ToString() });
            CheckSkillCondition(ConditionType.DEADLY_ATTACK, new List<string> { }, TimingType.DEADABLE_ATTACK);
            CheckSkillCondition(ConditionType.BE_ATTACKED, new List<string> { TargetType.ENEMY.GetDescription(), 1.ToString() });

            {
                var controller = CreateGameController();
                controller.CurrentPlayer.Team.AddBuff(controller.TargetPlayers, BuffType.STUN, 2);
                CheckSkillCondition(ConditionType.BE_ATTACKED_BUFF, new List<string> { BuffType.STUN.ToString() }, TimingType.NORMAL_ATTACK_AFTER, controller);
            }

            CheckSkillCondition(ConditionType.ATTACKER, new List<string> { TargetType.SELF.GetDescription() });

            {
                var controller = CreateGameController();

                var skill = new Skill();
                skill.Conditions.Add(new Logic.Entity.SkillCondition { ConditionType = ConditionType.ENEMY, Conditions = new List<string> { TargetType.ENEMY.GetDescription() } });

                controller.TargetPlayers[0].PassiveSkill.Add(skill);
                Assert.IsTrue(skill.ConditionCheck(TimingType.NORMAL_ATTACK_BEFORE, controller.TargetPlayers[0]));
            }

            CheckSkillCondition(ConditionType.ALLY, new List<string> { TargetType.ALLY.GetDescription() });
            CheckSkillCondition(ConditionType.BE_ATTACKED_CLASS_TYPE, new List<string> { ClassType.ATTACK.GetDescription() });
            CheckSkillCondition(ConditionType.ATTACKER_CLASS_TYPE, new List<string> { ClassType.DEFENSE.GetDescription() });
            CheckSkillCondition(ConditionType.COMBO_ATTACK, new List<string> { }, TimingType.COMBO_ATTACK);
            CheckSkillCondition(ConditionType.KILL_BY_ACTIVE_SKILL, new List<string> { }, TimingType.KILL_BY_ACTIVE_SKILL);
        }

        private void CheckSkillCondition(ConditionType conditionType, List<string> parameters, TimingType timingType = TimingType.NORMAL_ATTACK_BEFORE, GameController gameController = null)
        {
            if (gameController == null)
            {
                gameController = CreateGameController();
            }

            CheckConditionPlayer(gameController, gameController.CurrentPlayer, conditionType, parameters, timingType);
        }

        private void CheckSkillConditionEnemy(ConditionType conditionType, List<string> parameters, TimingType timingType = TimingType.NORMAL_ATTACK_BEFORE, GameController gameController = null)
        {
            if (gameController == null)
            {
                gameController = CreateGameController();
            }

            CheckConditionPlayer(gameController, gameController.TargetPlayers[0], conditionType, parameters, timingType);
        }

        private void CheckConditionPlayer(GameController gameController, Player player, ConditionType conditionType, List<string> parameters, TimingType timingType)
        {
            var skill = new Skill();
            skill.Conditions.Add(new Logic.Entity.SkillCondition
            {
                ConditionType = conditionType,
                Conditions = parameters
            });

            player.PassiveSkill.Add(skill);

            Assert.IsTrue(skill.ConditionCheck(timingType, player));
            ConditionTypes.Add(conditionType);
        }

        [Test, Order(3)]
        public void TestSkillEffect()
        {
            CheckSkillEffect(EffectType.NORMAL_ATTACK, new List<string> { });
            CheckSkillEffect(EffectType.NORMAL_ATTACK_COUNT, new List<string> { 3.ToString() });
            CheckSkillEffect(EffectType.NORMAL_ATTACK_RATIO, new List<string> { 120.ToString() });

            CheckSkillEffect(EffectType.BAD_STATUS, new List<string> { BadStatusType.AIR.ToString() });

            CheckSkillEffect(EffectType.DAMAGE_COUNT, new List<string> { 3.ToString() });
            CheckSkillEffect(EffectType.DAMAGE_RATIO, new List<string> { 15.ToString() });

            CheckSkillEffect(EffectType.ATTACK_SPEED_INCREASE, new List<string> { 20.ToString() });
            CheckSkillEffect(EffectType.ATTACK_SPEED_DECREASE, new List<string> { 20.ToString() });

            CheckSkillEffect(EffectType.ATTACK_INCREASE, new List<string> { 20.ToString() });
            CheckSkillEffect(EffectType.ATTACK_INCREASE_BY_BUFF_COUNT, new List<string> { 20.ToString() });

            CheckSkillEffect(EffectType.SKILL_COOLTIME_DECREASE, new List<string> { TargetType.SELF.GetDescription(), 2.ToString() });
            CheckSkillEffect(EffectType.SKILL_COOLTIME_INCREASE, new List<string> { TargetType.SELF.GetDescription(), 3.ToString() });

            CheckSkillEffect(EffectType.REMOVE_ALL_BUFF, new List<string> { });
            CheckSkillEffect(EffectType.COMBO_DAMAGE, new List<string> { });

            CheckSkillEffect(EffectType.INSTEAD, new List<string> { });
            CheckSkillEffect(EffectType.CREATE_COMBO, new List<string> { 100.ToString() });
            CheckSkillEffect(EffectType.COMBO_PROBABILITY, new List<string> { 100.ToString() });
            CheckSkillEffect(EffectType.DAMAGE_ALLEVIATE, new List<string> { 5.ToString() });
            CheckSkillEffect(EffectType.ADDITIONAL_ATTACK, new List<string> { 2.ToString() });
            CheckSkillEffect(EffectType.BUFF, new List<string> { BuffType.STUN.ToString(), 2.ToString() });
            CheckSkillEffect(EffectType.RECOVERY_HP, new List<string> { 5.ToString() });
            CheckSkillEffect(EffectType.RECOVERY_HP_BY_HP, new List<string> { 10.ToString() + CompareOperatorType.OVER.GetDescription(), 3.ToString(), 2.ToString() });
            CheckSkillEffect(EffectType.DECREASE_BUFF, new List<string> { BuffType.STUN.ToString() });
            CheckSkillEffect(EffectType.DAMAGE, new List<string> { 3.ToString() });
            CheckSkillEffect(EffectType.DAMAGE_BY_CLASS, new List<string> { ClassType.ATTACK.GetDescription(), 2.ToString(), 3.ToString() });
            CheckSkillEffect(EffectType.DAMAGE_BY_BUFF, new List<string> { BuffType.STUN.ToString(), 3.ToString(), 4.ToString() });
            CheckSkillEffect(EffectType.DAMAGE_BY_HP, new List<string> { 5.ToString() + CompareOperatorType.UNDER.GetDescription(), 5.ToString(), 6.ToString() });
            CheckSkillEffect(EffectType.ACTIVE_COOLTIME_DECREASE, new List<string> { 2.ToString() });
            CheckSkillEffect(EffectType.ACTIVE_COOLTIME_INCREASE, new List<string> { 3.ToString() });
            CheckSkillEffect(EffectType.COMBO_COOLTIME_DECREASE, new List<string> { 3.ToString() });

            CheckSkillEffect(EffectType.DAMAGE_BY_EXIST, new List<string> { "DEAD", 3.ToString(), 4.ToString() });
            CheckSkillEffect(EffectType.DAMAGE_BY_ORDER, new List<string> { 4.ToString(), 3.ToString(), 2.ToString(), 1.ToString() });
            CheckSkillEffect(EffectType.CHANGE_ORDER, new List<string> { });
        }

        private GameController CheckSkillEffect(EffectType effectType, List<string> parameters)
        {
            var controller = CreateGameController();

            var skill = new Skill();
            skill.Effects.Add(new Logic.Entity.SkillEffect { EffectType = effectType, Effects = parameters });

            controller.CurrentPlayer.ActiveSkill.Add(skill);

            Assert.IsTrue(skill.ConditionCheck(TimingType.NORMAL_ATTACK_BEFORE, controller.CurrentPlayer)); // 컨디션은 안넣어놨으므로, 무조건 유효하지만~!

            double damage = controller.CurrentPlayer.PlusAttack;
            Assert.IsTrue(skill.OnEffect(controller.CurrentPlayer));

            EffectTypes.Add(effectType);

            return controller;
        }

        [Test, Order(999)]
        public void TestCoverage()
        {
            CheckCoverage(EffectTypes, new HashSet<EffectType> { EffectType.BURN });
            CheckCoverage(TargetTypes, new HashSet<TargetType> { });
            CheckCoverage(ConditionTypes, new HashSet<ConditionType> { });
        }

        private static void CheckCoverage<T>(HashSet<T> hashSet, HashSet<T> whiteList) where T : struct
        {
            foreach (T e in (T[])Enum.GetValues(typeof(T)))
            {
                if (false == hashSet.Contains(e))
                {
                    Assert.IsTrue(whiteList.Contains(e));
                }
            }
        }
    }
}