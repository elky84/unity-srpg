using Logic.Entity;
using Logic.Protocols;
using Logic.Types;
using Logic.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Logic.Controller
{
    public static class GameStateExtend
    {
        public static void WriteEvent<T>(this GameState gameState, T evt, bool addSequence = true) where T : Event
        {
            if (addSequence)
            {
                gameState.AddSequence();
            }

            evt.Sequence = gameState.GetSequence();

            gameState.Events.Add(evt);
        }

        public static void EventsWriteToFile(this GameState gameState)
        {
            foreach (var evt in gameState.Events)
            {
                File.AppendAllText(@".\events.json", JsonConvert.SerializeObject(evt) + Environment.NewLine);
            }
        }

        public static void RunPassiveSkills(this GameState gameState, TimingType timing)
        {
            foreach (var player in gameState.ActivePlayers())
            {
                if (player.IsDead())
                {
                    continue;
                }

                foreach (var skill in player.PassiveSkill)
                {
                    if (skill.ConditionCheck(timing, player))
                    {
                        gameState.WriteEvent(new Protocols.SkillInvoke
                        {
                            PlayerIndex = player.PlayerIndex,
                            SkillType = SkillType.PASSIVE,
                            Skill = skill.ToProtocol(),
                            TargetPlayers = gameState.TargetPlayers.ToPlayerIndexes()
                        });

                        skill.OnEffect(player);
                    }
                }
            }
        }
    }
}
