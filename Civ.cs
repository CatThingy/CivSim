using System;

using DSharpPlus;
using DSharpPlus.Entities;


namespace CivSim
{
    public class Civ
    {
        public string Name { get; }
        public string Id { get; }

        public int Points { get; set; }
        public int Respec { get; set; }

        public int Offence { get; set; }
        public int Defence { get; set; }
        public int Research { get; set; }
        public int Education { get; set; }
        public int Healthcare { get; set; }

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

        public DiscordEmbed Format()
        {
            return new DiscordEmbedBuilder()
            .WithTitle(Name)
            .WithDescription($"Unspent points: {Points}\n Respecs available: {Respec}")
            .WithFooter($"Nation ID: {Id}")
            .AddField("Offence: " + this.Offence, "\u200b", true)
            .AddField("Defence: " + this.Defence, "\u200b", true)
            .AddField("\u200b", "\u200b")
            .AddField("Research: " + this.Research, "\u200b", true)
            .AddField("Education: " + this.Education, "\u200b", true)
            .AddField("\u200b", "\u200b")
            .AddField("Healthcare: " + this.Healthcare, "\u200b", true) ;
        }
    }
}