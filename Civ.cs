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
            {Stat.Offence, 0},
            {Stat.Defence, 0},
            {Stat.Research, 0},
            {Stat.Education, 0},
            {Stat.Healthcare, 0},
            {Stat.Civilian, 0},
            {Stat.Morale, 0},
        };
        // Derived stats
        [JsonIgnore]
        public int AttackMod { get => Stats[Stat.Offence] / 4; }
        [JsonIgnore]
        public int DefenceMod { get => Stats[Stat.Offence] / 4; }
        [JsonIgnore]
        public int WeeklyPoints { get => 10 + Stats[Stat.Offence] / 4; }
        [JsonIgnore]
        public int WeeklyRespecs { get => Math.Max(0, 10 + Stats[Stat.Offence] / 4); }
        // public int EventPenaltyMod { get => Healthcare / 4; }
        // public int MaxHealth { get => 100 + Civilian * 5; }
        // public int HealthRegen { get => 20 + Morale * 5; }

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

        public DiscordEmbed Format()
        {
            return new DiscordEmbedBuilder()
            .WithTitle(Name)
            .WithDescription($"Unspent points: {Points}\n Respecs available: {Respec}")
            .WithFooter($"Nation ID: {Id}")
            .AddField("Offence: " + Stats[Stat.Offence], "Attack modifier: " + AttackMod, true)
            .AddField("Defence: " + Stats[Stat.Defence], "Defence modifier: " + DefenceMod, true)
            .AddField("\u200b", "\u200b")
            .AddField("Research: " + Stats[Stat.Research], "Weekly points: " + WeeklyPoints, true)
            .AddField("Education: " + Stats[Stat.Education], "Weekly respecs: " + WeeklyRespecs, true)
            .AddField("\u200b", "\u200b")
            .AddField("Healthcare: " + Stats[Stat.Healthcare], "\u200b", true)
            .AddField("\u200b", "\u200b")
            .AddField("Civilian: " + Stats[Stat.Civilian], "\u200b", true)
            .AddField("Morale: " + Stats[Stat.Morale], "\u200b", true);
        }
    }
}