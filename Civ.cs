using System;

namespace CivSim
{
    public class Civ
    {
        public string Name { get; }

        public int Points { get; set; }
        public int Respec { get; set; }

        public int Offence { get; set; }
        public int Defence { get; set; }
        public int Research { get; set; }
        public int Education { get; set; }
        public int Healthcare { get; set; }

        public Civ(string name)
        {
            Name = name;
            Offence = 0;
            Defence = 0;
            Research = 0;
            Education = 0;
            Healthcare = 0;

            Points = 20;
            Respec = 10;
        }
    }
}