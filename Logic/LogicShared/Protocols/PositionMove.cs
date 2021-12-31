using Logic.Types;

namespace Logic.Protocols
{
    public class PositionMove : Event
    {
        public override EventType Type { get { return EventType.PositionMove; } }

        public int PlayerIndex { get; set; }

        public PlayerPosition PlayerPosition { get; set; }
    }
}
