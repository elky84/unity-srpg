using Logic.Code;
using Logic.Controller;
using Logic.MasterData;
using Logic.Protocols;

using Logic.Types;
using Logic.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Cli
{

    public class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static List<TeamSetting> TeamSettings { get; set; }

        private static List<PlayerData> PlayerDatas { get; set; }

        private static JsonSerializer JsonSerializer = new JsonSerializer();


        static void Main(string[] args)
        {
            string teamSettingJson = System.IO.File.ReadAllText(@"Json\TeamSetting.json");
            TeamSettings = JsonConvert.DeserializeObject<List<TeamSetting>>(teamSettingJson);

            string playerDataJson = System.IO.File.ReadAllText(@"Json\PlayerData.json");
            PlayerDatas = JsonConvert.DeserializeObject<List<PlayerData>>(playerDataJson);

            File.Delete(@"events.json");

            var config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("nlog.json", optional: true, reloadOnChange: true)
                .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));

            LogUtil.LogDelegates.Add(Logic.Types.LogLevel.Trace, new LogUtil.LogDelegate(logger.Trace));
            LogUtil.LogDelegates.Add(Logic.Types.LogLevel.Debug, new LogUtil.LogDelegate(logger.Debug));
            LogUtil.LogDelegates.Add(Logic.Types.LogLevel.Info, new LogUtil.LogDelegate(logger.Info));
            LogUtil.LogDelegates.Add(Logic.Types.LogLevel.Warn, new LogUtil.LogDelegate(logger.Warn));
            LogUtil.LogDelegates.Add(Logic.Types.LogLevel.Error, new LogUtil.LogDelegate(logger.Error));

            LogUtil.WriteInfo($"ProtocolVersion: {VersionUtil.ProtocolVersion()}");

            Play(false);

            //PerformanceTest();
        }

        static string[] UserInput(string playerLog)
        {
            LogUtil.WriteWithColor(Logic.Types.LogLevel.Debug, ConsoleColor.DarkBlue, ConsoleColor.DarkYellow, "현재플레이어 {0}. [스킬]과 [타겟]을 입력해주세요.", playerLog);
            LogUtil.WriteWithColor(Logic.Types.LogLevel.Debug, ConsoleColor.DarkBlue, ConsoleColor.DarkYellow, "예를 들어, \"0 1\" 이와 같이 입력한다면, [스킬:0, 타겟:1] 으로 인식 됩니다. 타겟인덱스는 입력하지 않으면 랜덤 선택합니다.", playerLog);
            return Console.ReadLine().Split(" ");
        }

        static void Play(bool interactiveMode)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var controller = new GameController();
            controller.Prepare(TeamSettings, PlayerDatas);

            var randomUtil = new RandomUtil();

            while (!controller.IsGameEnd())
            {
                if (controller.TurnStart())
                {
                    if (interactiveMode)
                    {
                        bool correctInput = false;
                        do
                        {
                            string[] inputs = UserInput(controller.CurrentPlayer.ToLog());
                            if (inputs[0].OnlyDigit().Length <= 0)
                            {
                                LogUtil.WriteWithColor(Logic.Types.LogLevel.Debug, ConsoleColor.DarkBlue, ConsoleColor.Red, "스킬 ID를 반드시 입력해야 합니다.");
                                continue;
                            }

                            int? targetIndex = null;
                            int skillId = inputs[0].ToInt();
                            if (inputs.Length >= 2 && inputs[1].OnlyDigit().Length > 0)
                            {
                                targetIndex = inputs[1].ToInt();
                            }
                            else
                            {
                                targetIndex = null;
                            }

                            var resultCode = controller.Action(skillId, out var needSelection);
                            if (resultCode == ResultCode.Success)
                            {
                                correctInput = true;
                                if (needSelection && controller.TargetPlayers.Count > 0)
                                {
                                    targetIndex = controller.TargetPlayers[randomUtil.Get(controller.TargetPlayers.Count)].PlayerIndex;
                                }
                                controller.Turn(skillId, targetIndex);
                            }
                            else
                            {
                                LogUtil.WriteWithColor(Logic.Types.LogLevel.Debug, ConsoleColor.DarkBlue, ConsoleColor.Red, resultCode.ToString());
                            }
                        }
                        while (correctInput == false);
                    }
                    else
                    {
                        controller.PlayAuto();
                    }
                }

                controller.EventsWriteToFile();
                var str = JsonConvert.SerializeObject(controller);
                controller = JsonConvert.DeserializeObject<GameController>(JsonConvert.SerializeObject(controller));
            }

            PlayEventFromJson();

            stopWatch.Stop();

            LogUtil.WriteInfo("Elapsed Time {0}", stopWatch.Elapsed.ToString());
        }

        static void PerformanceTest()
        {
            Play(false);

            {
                List<Logic.Protocols.Player> players = new List<Logic.Protocols.Player>();
                for (int x = 0; x < 4; ++x)
                {
                    players.Add(new Logic.Protocols.Player());
                }

                var gameStart = new GameStart();
                gameStart.Sequence = 0;

                gameStart.RedPlayers = players;
                gameStart.BluePlayers = players;

                int count = 10000;
                LogUtil.WriteInfo("Test Count {0}", count);

                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    for (int n = 0; n < count; ++n)
                    {
                        var json = JsonConvert.SerializeObject(gameStart);
                    }

                    stopWatch.Stop();

                    LogUtil.WriteInfo("JSON SO ElapsedTime {0}, {1}", stopWatch.Elapsed.Milliseconds, stopWatch.Elapsed);
                }

                {
                    var json = JsonConvert.SerializeObject(gameStart);
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    for (int n = 0; n < count; ++n)
                    {
                        JsonConvert.DeserializeObject<GameStart>(json);
                    }

                    stopWatch.Stop();

                    LogUtil.WriteInfo("JSON DO ElapsedTime {0}, {1}", stopWatch.Elapsed.Milliseconds, stopWatch.Elapsed);
                }

                {
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    for (int n = 0; n < count; ++n)
                    {
                        var json = JsonConvert.SerializeObject(gameStart);
                        JsonConvert.DeserializeObject<GameStart>(json);
                    }

                    stopWatch.Stop();

                    LogUtil.WriteInfo("JSON SO/DO ElapsedTime {0}, {1}", stopWatch.Elapsed.Milliseconds, stopWatch.Elapsed);
                }
            }
        }

        static private T PopulateFromExtensionData<T>(JObject extensionData) where T : new()
        {
            var value = new T();
            JsonSerializer.Populate(extensionData.CreateReader(), value);
            return value;
        }

        static void PlayEventFromJson()
        {
            foreach (var json in File.ReadLines(@"events.json"))
            {
                var evt = JsonConvert.DeserializeObject<Event>(json);
                switch (evt.Type)
                {
                    case EventType.GameStart:
                        {
                            var value = PopulateFromExtensionData<GameStart>(evt.ExtensionData);
                            LogUtil.WriteInfo("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.GameEnd:
                        {
                            var value = PopulateFromExtensionData<GameEnd>(evt.ExtensionData);
                            LogUtil.WriteInfo("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.PlayerTurn:
                        {
                            var value = PopulateFromExtensionData<PlayerTurn>(evt.ExtensionData);
                            LogUtil.WritePlayerTurn("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.SkillInvoke:
                        {
                            var value = PopulateFromExtensionData<SkillInvoke>(evt.ExtensionData);
                            LogUtil.WriteSkillEffect("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.BuffEffect:
                        {
                            var value = PopulateFromExtensionData<BuffEffect>(evt.ExtensionData);
                            LogUtil.WriteBuffEffect("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.BuffEffectRemove:
                        {
                            var value = PopulateFromExtensionData<BuffEffectRemove>(evt.ExtensionData);
                            LogUtil.WriteBuffEffect("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.NormalAttack:
                        {
                            var value = PopulateFromExtensionData<NormalAttack>(evt.ExtensionData);
                            LogUtil.WriteNormalAttack("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.ComboAttack:
                        {
                            var value = PopulateFromExtensionData<ComboAttack>(evt.ExtensionData);
                            LogUtil.WriteComboAttack("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.HpChange:
                        {
                            var value = PopulateFromExtensionData<HpChange>(evt.ExtensionData);
                            LogUtil.WriteHpChange("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.Guard:
                        {
                            var value = PopulateFromExtensionData<Guard>(evt.ExtensionData);
                            LogUtil.WriteGuard("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.BadStatus:
                        {
                            var value = PopulateFromExtensionData<BadStatus>(evt.ExtensionData);
                            LogUtil.WriteBadStatus("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.Dead:
                        {
                            var value = PopulateFromExtensionData<Dead>(evt.ExtensionData);
                            LogUtil.WriteBadStatus("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    case EventType.PositionMove:
                        {
                            var value = PopulateFromExtensionData<PositionMove>(evt.ExtensionData);
                            LogUtil.WriteBadStatus("Sequence: {0}, Value: {1}", evt.Sequence, value);
                        }
                        break;
                    default:
                        {
                            LogUtil.WriteImportant("Not Found EventType. Sequence: {0}, Type: {1}", evt.Sequence, evt.Type);
                        }
                        break;
                }
            }

        }
    }
}
