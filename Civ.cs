using System;

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
        public int Offence { get; set; }
        public int Defence { get; set; }
        public int Research { get; set; }
        public int Education { get; set; }
        public int Healthcare { get; set; }

        // Derived stats
        public int AttackMod { get => Offence / 4; }
        public int DefenceMod { get => Defence / 4; }
        public int WeeklyPoints { get => 10 + Research / 4; }
        public int WeeklyRespecs { get => Math.Max(0, 10 + Education / 4); }
        // public int EventPenaltyMod { get => Healthcare / 4;}

        public Civ(string name, string id)
        {
            Name = name;
            Id = id;

            Offence = 0;
            Defence = 0;
            Research = 0;
            Education = 0;
            Healthcare = 0;

            Points = 20;
            Respec = 10;
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
            .AddField("Offence: " + this.Offence, "Attack modifier: " + AttackMod, true)
            .AddField("Defence: " + this.Defence, "Defence modifier: " + DefenceMod, true)
            .AddField("\u200b", "\u200b")
            .AddField("Research: " + this.Research, "Weekly points: " + WeeklyPoints, true)
            .AddField("Education: " + this.Education, "Weekly respecs: " + WeeklyRespecs, true)
            .AddField("\u200b", "\u200b")
            .AddField("Healthcare: " + this.Healthcare, "\u200b", true);
        }
    }
}