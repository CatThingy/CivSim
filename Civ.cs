using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using DSharpPlus.Entities;


namespace CivSim
{
    public class Civ
    {
        public string Name { get; }
        public string Id { get; }

        public int Points { get; set; }
        public int Respec { get; set; }

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
        // public int EventPenaltyMod { get => Healthcare / 4; }
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

            // Apply events
            foreach (CivEvent e in Events)
            {
                if (e.Expiry < DateTime.Now)
                {
                    Events.Remove(e);
                    continue;
                }

                Effects[e.Stat] += e.Effect;
            }

            // Update effective stats
            foreach (Stat s in Enum.GetValues<Stat>())
            {
                EffectiveStats[s] = Stats[s] + Effects[s];
            }
        }

        string AddSign(int num)
        {
            return (num >= 0 ? "+" + num : num.ToString());
        }

        public DiscordEmbed Format()
        {
            return new DiscordEmbedBuilder()
            .WithTitle(Name)
            .WithDescription($"Unspent points: {Points}\n Respecs available: {Respec}")
            .WithFooter($"Nation ID: {Id}")
            .AddField("Offence: " + Stats[Stat.Offence] + $" ({AddSign(Effects[Stat.Offence])})", "Attack modifier: " + AttackMod, true)
            .AddField("Defence: " + Stats[Stat.Defence] + $" ({AddSign(Effects[Stat.Defence])})", "Defence modifier: " + DefenceMod, true)
            .AddField("Research: " + Stats[Stat.Research] + $" ({AddSign(Effects[Stat.Research])})", "Weekly points: " + WeeklyPoints, true)
            .AddField("Education: " + Stats[Stat.Education] + $" ({AddSign(Effects[Stat.Education])})", "Weekly respecs: " + WeeklyRespecs, true)
            .AddField("Healthcare: " + Stats[Stat.Healthcare] + $" ({AddSign(Effects[Stat.Healthcare])})", "\u200b", true)
            .AddField("Civilian: " + Stats[Stat.Civilian] + $" ({AddSign(Effects[Stat.Civilian])})", "\u200b", true)
            .AddField("Morale: " + Stats[Stat.Morale] + $" ({AddSign(Effects[Stat.Morale])})", "\u200b", true);
        }
    }
}