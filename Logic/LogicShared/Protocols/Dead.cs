using Logic.Types;

namespace Logic.Protocols
{
    public class Dead : Event
    {
        public override EventType Type { get { return EventType.Dead; } }

        public int PlayerIndex { get; set; }
    }
}
