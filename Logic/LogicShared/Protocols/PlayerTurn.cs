using Logic.Types;

namespace Logic.Protocols
{
    public class PlayerTurn : Event
    {
        public override EventType Type { get { return EventType.PlayerTurn; } }

        public int PlayerIndex { get; set; }
    }
}
