using System;
using Logic.Types;

namespace Logic.MasterData
{
    public class PlayerData
    {
        public string Skill4 { get; set; }
        public string Skill3 { get; set; }
        public string Skill2 { get; set; }
        public string Skill1 { get; set; }
        public int Range { get; set; }
        public int Move { get; set; }
        public double Guard { get; set; }
        public double Counter { get; set; }
        public double Critical { get; set; }
        public double Agility { get; set; }
        public double Defense { get; set; }
        public double Attack { get; set; }
        public double Hp { get; set; }
        public ClassType Type { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
