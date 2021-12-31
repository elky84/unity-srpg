using Logic.Types;

namespace Logic.Protocols
{
    public class GameEnd : Event
    {
        public override EventType Type { get { return EventType.GameEnd; } }

        public TeamType TeamType { get; set; }
    }
}
