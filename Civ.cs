using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using DSharpPlus.Entities;


namespace CivSim
{
    public class Civ
    {
        public string Name { get; set; }
        public string Id { get; }

        public int Points { get; set; }
        public int Respec { get; set; }

        public string Flag { get; set; }
        public string Colour { get; set; }

        // Base stats

        [JsonInclude]
        public Dictionary<Stat, int> Stats = new Dictionary<Stat, int>
        {
            { Stat.Offence, 0 },
            { Stat.Defence, 0 },
            { Stat.Research, 0 },
            { Stat.Education, 0 },
            { Stat.Healthcare, 0 },
            { Stat.Civilian, 0 },
            { Stat.Morale, 0 },
        };

        // Derived stats
        [JsonIgnore]
        public int AttackMod { get => EffectiveStats[Stat.Offence] / 4; }
        [JsonIgnore]
        public int DefenceMod { get => EffectiveStats[Stat.Defence] / 4; }
        [JsonIgnore]
        public int WeeklyPoints { get => 10 + EffectiveStats[Stat.Research] / 4; }
        [JsonIgnore]
        public int WeeklyRespecs { get => Math.Max(0, 10 + EffectiveStats[Stat.Education] / 4); }
        [JsonIgnore]
        public int EventPenaltyMod { get => Math.Max(0, (EffectiveStats[Stat.Healthcare] / 4) - (Effects.Count - 1)); }
        // public int MaxHealth { get => 100 + Civilian * 5; }
        // public int HealthRegen { get => 20 + Morale * 5; }
        [JsonInclude]
        public List<CivEvent> Events = new List<CivEvent>();

        [JsonIgnore]
        public Dictionary<Stat, int> Effects = new Dictionary<Stat, int>
        {
            { Stat.Offence, 0 },
            { Stat.Defence, 0 },
            { Stat.Research, 0 },
            { Stat.Education, 0 },
            { Stat.Healthcare, 0 },
            { Stat.Civilian, 0 },
            { Stat.Morale, 0 },
        };

        [JsonIgnore]
        public Dictionary<Stat, int> EffectiveStats = new Dictionary<Stat, int>()
        {
            { Stat.Offence, 0 },
            { Stat.Defence, 0 },
            { Stat.Research, 0 },
            { Stat.Education, 0 },
            { Stat.Healthcare, 0 },
            { Stat.Civilian, 0 },
            { Stat.Morale, 0 },
        };
        public Civ(string name, string id)
        {
            Name = name;
            Id = id;

            Points = 0;
            Respec = 0;
        }

        public void Update()
        {
            Points += WeeklyPoints;
            Respec = WeeklyRespecs;
        }

        public void UpdateEvents()
        {
            // Reset effects
            foreach (Stat s in Enum.GetValues<Stat>())
            {
                Effects[s] = 0;
            }

            int tempHealthcareMod = 0;

            // Apply healhtcare events
            foreach (CivEvent e in Events)
            {
                if (e.Expiry < DateTime.Now)
                {
                    Events.Remove(e);
                    continue;
                }
                if (e.Stat == Stat.Healthcare)
                    tempHealthcareMod += e.Effect;
            }
            // Update healthcare
            EffectiveStats[Stat.Healthcare] = Stats[Stat.Healthcare] + tempHealthcareMod;

            // Get all events with the worst case healthcare
            foreach (CivEvent e in Events)
            {
                Effects[e.Stat] += Math.Min(0, e.Effect + EventPenaltyMod);
            }
            foreach (Stat s in Enum.GetValues<Stat>())
            {
                EffectiveStats[s] = Stats[s] + Effects[s];
            }

        }
    }
}