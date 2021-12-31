using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Entity
{
    public class PlayerPosition
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Diff(PlayerPosition playerPosition) => Math.Abs(X - playerPosition.X) + Math.Abs(Y - playerPosition.Y);

        public Protocols.PlayerPosition ToProtocol()
        {
            return new Protocols.PlayerPosition { X = X, Y = Y };
        }

        public PlayerPosition Clone()
        {
            return new PlayerPosition { X = X, Y = Y };
        }
    }
}
