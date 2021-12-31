using Logic.Types;
using System.Collections.Generic;

namespace Logic.Protocols
{
    public class GameStart : Event
    {
        public override EventType Type { get { return EventType.GameStart; } }

        public List<Player> RedPlayers { get; set; }

        public List<Player> BluePlayers { get; set; }
    }
}
