using Logic.Code;
using Logic.MasterData;
using Logic.Entity;
using Logic.Exceptions;
using Logic.Protocols;

using Logic.Types;
using Logic.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Logic.Controller
{
    public class GameController : GameState
    {
        [JsonProperty]
        public int TurnCount { get; set; } = 0;

        public RandomUtil RandomUtil = new RandomUtil();

        public ResultCode Prepare(List<Logic.MasterData.TeamSetting> teamSettings, List<PlayerData> playerDataList)
        {
            var playerDatas = playerDataList.ToDictionary(playerData => playerData.Id);

            Teams.Add(new Team(TeamType.BLUE));
            Teams.Add(new Team(TeamType.RED));

            foreach (var teamSetting in teamSettings)
            {
                var team = Teams[teamSetting.TeamType.Code()];
                var player = new Entity.Player { PlayerIndex = teamSetting.Id - 1, PlayerData = playerDatas[teamSetting.PlayerId], Team = team, TeamType = team.TeamType, PlayerPosition = new Entity.PlayerPosition { X = teamSetting.PositionX, Y = teamSetting.PositionY } };
                player.Setup(team);
            }

            foreach (var team in Teams)
            {
                if (team.Players.Count <= 0)
                {
                    if (team.TeamType == TeamType.BLUE)
                    {
                        return ResultCode.BlueTeamIsEmpty;
                    }
                    else if (team.TeamType == TeamType.RED)
                    {
                        return ResultCode.RedTeamIsEmpty;
                    }
                }
            }

            this.WriteEvent(new GameStart
            {
                RedPlayers = GetTeam(TeamType.RED).Players.ConvertAll(player => player.ToProtocol()),
                BluePlayers = GetTeam(TeamType.BLUE).Players.ConvertAll(player => player.ToProtocol())
            });

            LogUtil.WriteGameStartEnd("[게임시작]");
            return ResultCode.Success;
        }

        public Team GetTeam(TeamType teamType)
        {
            return Teams[teamType.Code()];
        }

        public ResultCode Action(int skillId, out bool needSelection)
        {
            SetTeamAndGameState();

            TargetPlayers.Clear();

            var skill = CurrentPlayer.GetSkill(skillId);
            if (skill == null)
            {
                needSelection = false;
                return ResultCode.InvalidSkillId;
            }

            if (!skill.ConditionCheck(TimingType.ACTIVE_SKILL_BEFORE, CurrentPlayer))
            {
                needSelection = false;
                return ResultCode.SkillCooltime;
            }

            var targets = GetTargets(CurrentPlayer, skill.Target.TargetType, skill.Target.Targets, out needSelection);
            TargetPlayers.AddRange(targets);
            return ResultCode.Success;
        }


        public bool TurnStart()
        {
            SetTeamAndGameState();

            do
            {
                CurrentPlayer = GetCurrentTurnPlayer();
                // Speed 100 이상인 플레이어가 없을경우
                if (CurrentPlayer == null)
                {
                    // 자신의 Speed 만큼 증가
                    IncreaseSpeed(1.0f);
                }
            } while (CurrentPlayer == null);

            CurrentPlayerIndex = CurrentPlayer.PlayerIndex;

            CurrentPlayer.Speed -= 100;
            this.WriteEvent(new PlayerTurn { PlayerIndex = CurrentPlayer.PlayerIndex });

            ProcessBuff();

            //TODO 현재 플레이어가 스턴 상태이면 스킵?
            if (CurrentPlayer.HasBuff(BuffType.STUN))
            {
                return false;
            }

            // 사용 가능 스킬이 없다면 패스
            if (CurrentPlayer.GetUsableSkill().Count <= 0)
            {
                return false;
            }

            LogUtil.WritePlayerTurn("[플레이어턴변경] Team:{0}, TurnCount:{1}, Player:{2}", CurrentTeam, TurnCount, CurrentPlayer.ToLog());
            // TODO BLUE 팀이 유저라고 가정한 하드 코딩.
            if (CurrentPlayer.TeamType != TeamType.BLUE)
            {
                PlayAuto();
                return false;
            }

            return true;
        }

        public bool CanMove(int playerIndex, Entity.PlayerPosition playerPosition)
        {
            foreach (var alivePlayer in GetAlivePlayers())
            {
                if (alivePlayer.PlayerIndex == playerIndex)
                {
                    continue;
                }

                if (0 == alivePlayer.PlayerPosition.Diff(playerPosition))
                {
                    return false;
                }
            }
            return true;
        }

        private bool MoveAndRollback(int playerIndex, Entity.PlayerPosition playerPosition, int x, int y, ref int move)
        {
            var gap = (Math.Abs(x) + Math.Abs(y));
            if (move < gap)
            {
                return false;
            }

            playerPosition.X += x;
            playerPosition.Y += y;

            if (false == CanMove(playerIndex, playerPosition))
            {
                playerPosition.X -= x;
                playerPosition.Y -= y;
                return false;
            }

            move -= gap;
            return true;
        }

        public Entity.PlayerPosition CalcTargetPosition()
        {
            var targetPosition = CurrentPlayer.PlayerPosition.Clone();
            if (TargetPlayers.Any())
            {
                var targetPlayer = TargetPlayers[0];
                var move = CurrentPlayer.PlayerData.Move;
                while (0 < move)
                {
                    if (targetPosition.Diff(targetPlayer.PlayerPosition) <= CurrentPlayer.PlayerData.Range)
                    {
                        break;
                    }

                    if (targetPosition.X > targetPlayer.PlayerPosition.X)
                    {
                        if (MoveAndRollback(CurrentPlayer.PlayerIndex, targetPosition, -1, 0, ref move))
                        {
                            continue;
                        }
                    }

                    if (targetPosition.X < targetPlayer.PlayerPosition.X)
                    {
                        if (MoveAndRollback(CurrentPlayer.PlayerIndex, targetPosition, 1, 0, ref move))
                        {
                            continue;
                        }
                    }

                    if (targetPosition.Y > targetPlayer.PlayerPosition.Y)
                    {
                        if (MoveAndRollback(CurrentPlayer.PlayerIndex, targetPosition, 0, -1, ref move))
                        {
                            continue;
                        }
                    }

                    if (targetPosition.Y < targetPlayer.PlayerPosition.Y)
                    {
                        if (MoveAndRollback(CurrentPlayer.PlayerIndex, targetPosition, 0, 1, ref move))
                        {
                            continue;
                        }
                    }

                    break;
                }
            }
            return targetPosition;
        }

        public void PlayAuto()
        {
            var skill = CurrentPlayer.RandomSKill();
            if (skill == null)
            {
                return;
            }

            int? targetIndex = null;
            if (ResultCode.Success == Action(skill.SkillId, out var needSelection))
            {
                if (needSelection && TargetPlayers.Count > 0)
                {
                    targetIndex = TargetPlayers[RandomUtil.Get(TargetPlayers.Count)].PlayerIndex;
                }
                Turn(skill.SkillId, targetIndex);
            }
        }

        private void InitTurnParameters()
        {
            ComboCount = 0;
            CurrentPlayer.DecreaseDamage = 0;
            CurrentPlayer.CriticalAttack = false;
        }

        private void SetTeamAndGameState()
        {
            foreach (var team in Teams)
            {
                team.GameState = this;
                foreach (var player in team.Players)
                {
                    player.Team = team;
                    player.GameState = this;

                    if (player.PlayerIndex == CurrentPlayerIndex)
                    {
                        CurrentPlayer = player;
                    }
                }
            }
        }

        public void Turn(int skillId, int? targetIndex)
        {
            SetTeamAndGameState();

            try
            {
                PlayerTurnStart();

                var skill = CurrentPlayer.GetSkill(skillId);
                //대상이 없는 경우 (남은 적군이 히든 상태인 경우)
                if (TargetPlayers.Any())
                {
                    if (targetIndex.HasValue)
                    {
                        var targetPlayer = TargetPlayers.Where(x => x.PlayerIndex == targetIndex.Value).FirstOrDefault();
                        if (targetPlayer != null)
                        {
                            TargetPlayers.Clear();
                            TargetPlayers.Add(targetPlayer);
                        }
                    }

                    CurrentPlayer.Move(CalcTargetPosition());

                    var nearPlayer = TargetPlayers.OrderBy(x => CurrentPlayer.PlayerPosition.Diff(x.PlayerPosition)).First();
                    if (nearPlayer.PlayerPosition.Diff(CurrentPlayer.PlayerPosition) > CurrentPlayer.PlayerData.Range)
                    {
                        LogUtil.WriteInfo($"사거리 내에 적이 타게팅된 적이 아무도 없음. {skillId}");
                        return;
                    }

                    this.WriteEvent(new Protocols.SkillInvoke
                    {
                        PlayerIndex = CurrentPlayer.PlayerIndex,
                        SkillType = SkillType.ACTIVE,
                        Skill = skill.ToProtocol(),
                        TargetPlayers = this.TargetPlayers.ToPlayerIndexes()
                    });
                    CurrentPlayer.ActiveSkill(skill);
                }
                else
                {
                    LogUtil.WriteInfo($"타겟이 없음: {skillId}");
                }

                TurnEnd();
            }
            catch (GameEndException exception)
            {
                this.WriteEvent(new GameEnd { TeamType = exception.TeamType });

                LogUtil.WriteGameStartEnd("[게임종료] Team:{0}, TurnCount:{1}", exception.TeamType, TurnCount);
            }
        }

        private void TurnEnd()
        {
            PlayerTurnEnd();

            IncreaseSpeed(0.07f);

            TurnCount += 1;
        }

        private void PlayerTurnStart()
        {
            InitTurnParameters();
        }

        private void ProcessBuff()
        {
            foreach (var buff in CurrentPlayer.GetBuffs(BuffType.BURN))
            {
                CurrentPlayer.Damage(CurrentPlayer, 10, EffectType.BURN);
            }

            CurrentPlayer.DecreaseBuffCooltime(BuffType.ALL, 1);
            CurrentPlayer.DecreaseSkillCooltime(1);
        }

        private void PlayerTurnEnd()
        {
            foreach (var player in ActivePlayers())
            {
                if (player.BadStatusType != BadStatusType.NONE)
                {
                    this.WriteEvent(new BadStatus { PlayerIndex = player.PlayerIndex, TargetPlayers = new List<int> { player.PlayerIndex }, BadStatusType = BadStatusType.NONE });
                    player.BadStatusType = BadStatusType.NONE;
                }
            }
        }
    }
}
