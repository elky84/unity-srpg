using Logic.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Logic.Protocols
{
    public class Player
    {
        public int PlayerIndex { get; set; }

        public TeamType TeamType { get; set; }

        public double Hp { get; set; }

        public int Id { get; set; }

        public double Attack { get; set; }

        public int Speed { get; set; }

        public List<Skill> PassiveSkill { get; set; }

        public List<Skill> ActiveSkill { get; set; }

        public PlayerPosition PlayerPosition { get; set; }
    }
}
