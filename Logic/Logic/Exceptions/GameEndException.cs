using Logic.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Exceptions
{
    public class GameEndException : Exception
    {
        public TeamType TeamType { get; set; }
    }
}
