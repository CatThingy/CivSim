using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CivSim
{
    public class War
    {
        // The two sides of the war - contains civ IDs
        [JsonInclude]
        public List<string> A = new List<string>();

        [JsonInclude]
        public List<string> B = new List<string>();

        public DateTime Start { get; }

        public War()
        {
            Start = DateTime.Today;
        }

    }
}