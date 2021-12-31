using Logic.Entity;
using Logic.Protocols;
using Logic.Types;
using Logic.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logic.Controller
{
    //TODO 게임 진행 상태, 참고 값을 담는 클래스. 자주 호출되는데다가, Entity.Player, Team에 주입해둬서 환경변수 참고용 코드가 지저분해짐. 이를 어찌 해결 할 것인가?
    [JsonObject(MemberSerialization.OptIn)]
    public class GameState
    {
        [JsonProperty]
        public int CurrentPlayerIndex { get; set; }

        public Entity.Player CurrentPlayer { get; set; }

        [JsonProperty]
        protected List<Team> Teams = new List<Team>();

        [JsonProperty]
        public TeamType CurrentTeamType { get; set; }

        [JsonProperty]
        private int Sequence { get; set; }

        public Team CurrentTeam => Teams[CurrentPlayer.TeamType.Code()];

        public Team EnemyTeam => Teams[(CurrentTeam.TeamType == TeamType.BLUE ? TeamType.RED : TeamType.BLUE).Code()];

        public List<Entity.Player> TargetPlayers { get; set; } = new List<Entity.Player>();


        public int ComboCount { get; set; } = 0;

        public bool InvertSortType = false;


        public List<Event> Events { get; set; } = new List<Event>();

        public void AddSequence()
        {
            Sequence += 1;
        }

        public int GetSequence()
        {
            return Sequence;
        }

        //TODO 로직적으로는 합리적이나, 매번 호출 할 때마다 조건 검사를 하는 것이 찜찜함. 스턴 등의 상태 이상에 빠지는 사용자를 매번 계산해놓기엔 결합도가 커져서 우선 코스트를 사용.
        public List<Entity.Player> ActivePlayers()
        {
            return new List<Entity.Player>().Concat(CurrentTeam.PlayablePlayers).Concat(EnemyTeam.PlayablePlayers).ToList();
        }
        public List<Entity.Player> GetAlivePlayers()
        {
            var alivePlayers = new List<Entity.Player>();
            foreach (var team in Teams)
            {
                alivePlayers.AddRange(team.AlivePlayers);
            }
            return alivePlayers;
        }

        public Team GetVictoryTeam()
        {
            foreach (var team in Teams)
            {
                if (0 < team.AlivePlayers.Count)
                {
                    return team;
                }
            }

            return null;
        }


        public bool IsGameEnd()
        {
            foreach (var team in Teams)
            {
                if (0 >= team.AlivePlayers.Count)
                {
                    return true;
                }
            }
            return false;
        }

        public Entity.Player GetCurrentTurnPlayer()
        {
            var alivePlayers = GetAlivePlayers();
            return alivePlayers.Where(x => x.Speed >= 100).OrderByDescending(x => x.Speed).FirstOrDefault();
        }

        public Entity.Player GetPlayer(int playerIndex)
        {
            return GetAlivePlayers().Where(x => x.PlayerIndex == playerIndex).FirstOrDefault();
        }

        public void IncreaseSpeed(float rate)
        {
            foreach (var team in Teams)
            {
                foreach (var player in team.AlivePlayers)
                {
                    float multiply = 1f;
                    foreach (var buff in player.GetBuffs(BuffType.ATTACK_SPEED_UP))
                    {
                        multiply += buff.Value;
                    }
                    player.Speed += (int)((player.PlayerData.Agility * multiply) * rate);
                }
            }
        }

        public TeamType TargetTypeToTeamType(TeamType teamType, TargetType targetType)
        {
            // 적군에서 찾아야 되는 경우 반드시 여기 조건에 포함되어야 한다.
            if (targetType == TargetType.ENEMY ||
                targetType == TargetType.ENEMY_ALL ||
                targetType == TargetType.WITHOUT_CLASS_ENEMY ||
                targetType == TargetType.LOWHP_ENEMY ||
                targetType == TargetType.HIGHHP_ENEMY ||
                targetType == TargetType.CLASS_ENEMY ||
                targetType == TargetType.ENEMY_EVERYONE ||
                targetType == TargetType.ENEMY_ORDER_BY_HP ||
                targetType == TargetType.ENEMY_SELECT ||
                targetType == TargetType.ENEMY_BY_BUFF)
            {
                return teamType == TeamType.BLUE ? TeamType.RED : TeamType.BLUE;
            }
            else
            {
                return teamType == TeamType.BLUE ? TeamType.BLUE : TeamType.RED;
            }
        }

        public List<Entity.Player> GetTargets(Entity.Player self, TargetType targetType, List<string> parameters, out bool needSelection)
        {
            var result = new List<Entity.Player>();
            var targetTeamType = TargetTypeToTeamType(self.TeamType, targetType);
            needSelection = false;
            var targetTeam = Teams[(int)targetTeamType];
            if (targetType == TargetType.SELF)
            {
                result.Add(self);
            }
            else if (targetType == TargetType.ENEMY || targetType == TargetType.ALLY)
            {
                var TargetPlayers = targetTeam.AlivePlayers.ToList();
                self.RandomUtil.Shuffle(TargetPlayers);
                int count = int.Parse(parameters[0]);
                for (int i = 0; i < Math.Min(count, TargetPlayers.Count); ++i)
                {
                    result.Add(TargetPlayers[i]);
                }
            }
            else if (targetType == TargetType.ENEMY_ALL || targetType == TargetType.ALLY_ALL || targetType == TargetType.ALLY_EVERYONE || targetType == TargetType.ENEMY_EVERYONE)
            {
                result.AddRange(targetTeam.AlivePlayers);
            }
            else if (targetType == TargetType.ALLY_ALL_EXCEPT_SELF)
            {
                result.AddRange(targetTeam.AlivePlayers);
                result.Remove(self);
            }
            else if (targetType == TargetType.LOWHP_ENEMY)
            {
                result.Add(targetTeam.AlivePlayers.OrderBy(x => x.GetHp()).FirstOrDefault());
            }
            else if (targetType == TargetType.HIGHHP_ENEMY)
            {
                result.Add(targetTeam.AlivePlayers.OrderByDescending(x => x.GetHp()).FirstOrDefault());
            }
            else if (targetType == TargetType.ENEMY_ORDER_BY_HP)
            {
                var sortType = TypesUtil.FromDescription<SortType>(parameters[0]);
                if (InvertSortType)
                {
                    sortType = sortType.Value == SortType.ASCENDING ? SortType.DESCENDING : SortType.ASCENDING;
                    InvertSortType = false;
                }

                if (sortType.Value == SortType.ASCENDING)
                {
                    result.AddRange(targetTeam.AlivePlayers.OrderBy(x => x.GetHp()));
                }
                else
                {
                    result.AddRange(targetTeam.AlivePlayers.OrderByDescending(x => x.GetHp()));
                }
            }
            else if (targetType == TargetType.CLASS_ENEMY || targetType == TargetType.CLASS_ALLY)
            {
                var classType = TypesUtil.FromDescription<ClassType>(parameters[0]);
                result.AddRange(targetTeam.AlivePlayers.Where(x => x.Type == classType.Value));
            }
            else if (targetType == TargetType.WITHOUT_CLASS_ENEMY || targetType == TargetType.WITHOUT_CLASS_ALLY)
            {
                var classType = TypesUtil.FromDescription<ClassType>(parameters[0]);
                result.AddRange(targetTeam.AlivePlayers.Where(x => x.Type != classType.Value));
            }
            else if (targetType == TargetType.ALLY_BY_BUFF || targetType == TargetType.ENEMY_BY_BUFF)
            {
                var buffType = TypesUtil.FromDescription<BuffType>(parameters[0]);
                result.AddRange(targetTeam.AlivePlayers.Where(x => x.HasBuff(buffType.Value)));
            }
            else if (targetType == TargetType.ALLY_SELECT || targetType == TargetType.ENEMY_SELECT)
            {
                result.AddRange(targetTeam.AlivePlayers);
                needSelection = true;
            }
            else
            {
                result.AddRange(Teams[(int)TeamType.BLUE].AlivePlayers);
                result.AddRange(Teams[(int)TeamType.RED].AlivePlayers);
            }
            return result;

        }
    }
}
