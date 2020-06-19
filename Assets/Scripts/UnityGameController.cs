using System.Collections.Generic;
using UnityEngine;

using Logic.Controller;
using Logic.Protocols;

using Newtonsoft.Json;
using Logic.Util;
using System.Linq;
using System.Collections;
using UnityEngine.UI;
using Logic.MasterData;

public class UnityGameController : MonoBehaviour
{
    private GameController controller = new Logic.Controller.GameController();

    private List<TeamSetting> TeamSettings { get; set; }

    private Dictionary<int, PlayerData> PlayerDatas { get; set; }

    private Dictionary<int, PlayerDataClient> PlayerDataClients { get; set; }

    private List<Character> Characters { get; set; } = new List<Character>();

    private List<Logic.Protocols.Event> UnProcessEvents { get; set; }

    private NoticeText NoticeText { get; set; }

    private RandomUtil RandomUtil = new RandomUtil();

    private Logic.Protocols.Event KeepEvent { get; set; }

    private Text GameSpeedButtonText { get; set; }

    public Transform ProjectTilesTransform { get; set; }

    public Transform CharactersTransform { get; set; }

    public Transform RangesTransform { get; set; }


    void Awake()
    {
        GameSpeedButtonText = transform.Find("GameSpeed/Button/Text").GetComponent<Text>();
        CharactersTransform = transform.Find("Characters");
        ProjectTilesTransform = transform.Find("ProjectTiles");
        RangesTransform = transform.Find("Ranges");
    }

    void Start()
    {
        NoticeText = GetComponentInChildren<NoticeText>();

        TeamSettings = JsonConvert.DeserializeObject<List<TeamSetting>>(System.IO.File.ReadAllText(@"Assets\Resources\Json\TeamSetting.json"));

        var playerDatas = JsonConvert.DeserializeObject<List<PlayerData>>(System.IO.File.ReadAllText(@"Assets\Resources\Json\PlayerData.json"));

        PlayerDataClients = JsonConvert.DeserializeObject<List<PlayerDataClient>>(System.IO.File.ReadAllText(@"Assets\Resources\Json\PlayerDataClient.json")).ToDictionary(x => x.Id);

        PlayerDatas = playerDatas.ToDictionary(x => x.Id);

        controller.Prepare(TeamSettings, playerDatas);

        while (!controller.IsGameEnd())
        {
            if (controller.TurnStart())
            {
                controller.PlayAuto();
            }
        }

        UnProcessEvents = new List<Logic.Protocols.Event>(controller.Events);
        StartCoroutine(PopEvent());
    }

    public void ChangeTimeScale()
    {
        Time.timeScale = Time.timeScale * 2f;
        if (Time.timeScale > 32f)
        {
            Time.timeScale = 1f;
        }

        GameSpeedButtonText.text = "GameSpeed X " + Time.timeScale.ToString();
    }

    public void OnThrowing()
    {
        if (KeepEvent == null)
        {
            return;
        }

        switch (KeepEvent.Type)
        {
            case Logic.Types.EventType.NormalAttack:
                {
                    var value = (NormalAttack)KeepEvent;
                    var character = Characters[value.PlayerIndex];

                    foreach (var target in value.TargetPlayers)
                    {
                        var targetPlayer = Characters[target];
                        StartCoroutine(character.Shot(targetPlayer.transform.localPosition.ToPlayerPosition()));
                    }
                }
                break;
            case Logic.Types.EventType.ComboAttack:
                {
                    var value = (ComboAttack)KeepEvent;
                    var character = Characters[value.PlayerIndex];
                    foreach (var target in value.TargetPlayers)
                    {
                        var targetPlayer = Characters[target];
                        StartCoroutine(character.Shot(targetPlayer.transform.localPosition.ToPlayerPosition()));
                    }
                }
                break;
        }
    }

    public void OnAttacking()
    {
        if (KeepEvent == null)
        {
            return;
        }

        var eventGroups = new List<Logic.Protocols.Event>(UnProcessEvents).GroupBy(e => e.Sequence);
        var deads = eventGroups.First().Where(evt => evt.Type == Logic.Types.EventType.Dead).ToList().Count > 0 ? true : false;
        if (deads)
        {
            KeepEvent = null;
            StartCoroutine(PopEvent());
            return;
        }

        switch (KeepEvent.Type)
        {
            case Logic.Types.EventType.NormalAttack:
                {
                    var value = (NormalAttack)KeepEvent;
                    var character = Characters[value.PlayerIndex];

                    foreach (var target in value.TargetPlayers)
                    {
                        Characters[target].Damage();
                    }
                }
                break;
            case Logic.Types.EventType.ComboAttack:
                {
                    var value = (ComboAttack)KeepEvent;
                    var character = Characters[value.PlayerIndex];

                    foreach (var target in value.TargetPlayers)
                    {
                        Characters[target].Damage();
                    }
                }
                break;
        }

        KeepEvent = null;
    }

    public void TurnOffRanges()
    {
        foreach (Transform child in RangesTransform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }


    public IEnumerator PopEvent()
    {
        if (UnProcessEvents.Count <= 0)
        {
            yield return null;
        }

        bool loop = true;
        foreach (var evt in new List<Logic.Protocols.Event>(UnProcessEvents))
        {
            if (loop == false)
            {
                break;
            }

            //#if UNITY_EDITOR
            //            StartCoroutine(NoticeText.ShowText(evt.Type.ToString(), 1f));
            //#endif

            switch (evt.Type)
            {
                case Logic.Types.EventType.GameStart:
                    {
                        var value = (GameStart)evt;
                        for (int n = 0; n < value.BluePlayers.Count; ++n)
                        {
                            var player = value.BluePlayers[n];
                            var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, player.PlayerPosition.PlayerPositionToVector3(), Quaternion.identity, CharactersTransform);

                            var character = gameObj.GetComponent<Character>();
                            character.PlayerData = PlayerDatas[player.Id];
                            character.PlayerDataClient = PlayerDataClients[player.Id];
                            character.SetTeam(Logic.Types.TeamType.BLUE);
                            character.Controller = this;

                            Characters.Add(character);
                        }


                        for (int n = 0; n < value.RedPlayers.Count; ++n)
                        {
                            var player = value.RedPlayers[n];

                            var gameObj = Instantiate(Resources.Load("Prefabs/Character") as GameObject, player.PlayerPosition.PlayerPositionToVector3(), Quaternion.identity, CharactersTransform);
                            var character = gameObj.GetComponent<Character>();
                            character.FlipX = true;
                            character.PlayerData = PlayerDatas[player.Id];
                            character.PlayerDataClient = PlayerDataClients[player.Id];
                            character.SetTeam(Logic.Types.TeamType.RED);
                            character.Controller = this;

                            Characters.Add(character);
                        }
                    }
                    break;
                case Logic.Types.EventType.GameEnd:
                    {
                        var value = (GameEnd)evt;
                        // 콩그레츄 레이숀~
                        TurnOffRanges();
                    }
                    break;
                case Logic.Types.EventType.PlayerTurn:
                    {
                        var value = evt as PlayerTurn;

                        yield return new WaitForSeconds(0.5f);
                        foreach (var player in Characters)
                        {
                            player.SetTurnObject(false);
                        }

                        var character = Characters[value.PlayerIndex];
                        character.SetTurnObject(true);

                        loop = false;
                    }
                    break;
                case Logic.Types.EventType.SkillInvoke:
                    {
                        var value = (SkillInvoke)evt;

                        var character = Characters[value.PlayerIndex];
                        //character.ShowStateText(string.Join(",", value.Skill.Effects.ConvertAll(e => e.EffectType.ToString())));
                    }
                    break;
                case Logic.Types.EventType.BuffEffect:
                    {
                        var value = (BuffEffect)evt;

                        foreach (var target in value.TargetPlayers)
                        {
                            var targetPlayer = Characters[target];
                            targetPlayer.ShowBuffText(value.Buff.Type.ToString());
                        }
                    }
                    break;
                case Logic.Types.EventType.BuffEffectRemove:
                    {
                        var value = (BuffEffectRemove)evt;

                        var character = Characters[value.PlayerIndex];
                        character.ShowBuffText("");
                    }
                    break;
                case Logic.Types.EventType.NormalAttack:
                    {
                        var value = (NormalAttack)evt;

                        var character = Characters[value.PlayerIndex];
                        character.Attack();
                        KeepEvent = evt;
                        loop = false;
                    }
                    break;
                case Logic.Types.EventType.ComboAttack:
                    {
                        var value = (ComboAttack)evt;
                        var character = Characters[value.PlayerIndex];
                        character.Attack();
                        KeepEvent = evt;
                        loop = false;
                    }
                    break;
                case Logic.Types.EventType.HpChange:
                    {
                        var value = (HpChange)evt;

                        var character = Characters[value.DestPlayerIndex];
                        character.HpChange(-value.Value);
                        character.ShowDamageText(value.Value);
                    }
                    break;
                case Logic.Types.EventType.Dead:
                    {
                        var value = (Dead)evt;

                        var character = Characters[value.PlayerIndex];
                        character.Dead();
                        loop = false;
                    }
                    break;
                case Logic.Types.EventType.BadStatus:
                    {
                        var value = (BadStatus)evt;

                        foreach (var target in value.TargetPlayers)
                        {
                            var targetPlayer = Characters[target];
                            targetPlayer.ShowStateText(value.BadStatusType.ToString());
                        }
                    }
                    break;
                case Logic.Types.EventType.PositionMove:
                    {
                        var value = (PositionMove)evt;

                        var character = Characters[value.PlayerIndex];
                        StartCoroutine(character.Move(value.PlayerPosition.ToEntity()));
                        loop = false;
                    }
                    break;
                default:
                    LogUtil.WriteError("Not found EventType. {0}, {1}", evt.Type, evt.Sequence);
                    break;
            }

            UnProcessEvents.Remove(evt);
        }
    }
}