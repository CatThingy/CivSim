using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace CivSim
{
    public class CivManagementModule : BaseCommandModule
    {
        string GetUserHash(ulong id)
        {
            HashAlgorithm algorithm = SHA1.Create();
            byte[] rawHash = algorithm.ComputeHash(BitConverter.GetBytes(id));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in rawHash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString()[^8..];
        }

        bool UserExists(string userHash)
        {
            return CivManager.Instance.Civs.ContainsKey(userHash);
        }

        // Levels 0-25: costs 1 point
        // Levels 26-50: costs 2 points
        // Levels 51-75: costs 3 points
        // etc.
        int computePointDifference(int from, int to)
        {
            int l1 = Math.Abs(from) / 25;
            int l2 = Math.Abs(to) / 25;

            return Math.Sign(to) * (((l2 * (l2 + 1)) / 2) * 25 + (Math.Abs(to) % 25) * (l2 + 1)) - Math.Sign(from) * (((l1 * (l1 + 1)) / 2) * 25 + (Math.Abs(from) % 25) * (l1 + 1));
        }

        // Inverse function of above. See https://www.desmos.com/calculator/h4wsckmdoa
        public int maxPointDifference(int from, int points)
        {
            int k1 = (int)Math.Ceiling((-5 + Math.Sqrt(26 + 8 * Math.Abs(from + points))) / 10d);
            int k2 = (int)Math.Ceiling((-5 + Math.Sqrt(26 + 8 * Math.Abs(from))) / 10d);
            return (int)(Math.Sign(points) * ((((Math.Abs(from + points)) / k1) + 12.5 * (k1 - 1)) - Math.Abs(from) / k2 + 12.5 * (k2 - 1)));
        }

        //TODO: Replace this whole thing with a slash command so that other people can't make the mistake of interacting with it
        [Command("create"), Aliases("register")]
        public async Task CreateCiv(CommandContext context, params string[] name)
        {
            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (UserExists(userHash))
            {
                await context.RespondAsync("You've already registered.");
                return;
            }
            Civ newCiv = new Civ(String.Join(" ", name), userHash);

            Dictionary<string, DiscordSelectComponentOption> stats = new Dictionary<string, DiscordSelectComponentOption>()
            {
                {"offence", new DiscordSelectComponentOption("Offence", "offence")},
                {"defence", new DiscordSelectComponentOption("Defence", "defence")},
                {"research", new DiscordSelectComponentOption("Research", "research")},
                {"education", new DiscordSelectComponentOption("Education", "education")},
                {"healthcare", new DiscordSelectComponentOption("Healthcare", "healthcare")},
            };

            List<DiscordSelectComponentOption> options = new List<DiscordSelectComponentOption>()
            {
                stats["offence"],
                stats["defence"],
                stats["research"],
                stats["education"],
                stats["healthcare"]
            };

            DiscordMessageBuilder statMessage = new DiscordMessageBuilder()
            .WithContent($"Choose your main stats:")
            .AddComponents(new DiscordSelectComponent("stat", "Select...", options));

            DiscordMessage msg = await context.RespondAsync(statMessage);

            InteractivityResult<ComponentInteractionCreateEventArgs> result = new InteractivityResult<ComponentInteractionCreateEventArgs>();

            int amt = 16;

            for (int i = 0; i < 3; i++)
            {
                result = await msg.WaitForSelectAsync(context.User, "stat", null);

                if (result.TimedOut)
                {
                    await msg.DeleteAsync();
                    return;
                }
                options.Remove(stats[result.Result.Values[0]]);

                switch (result.Result.Values[0])
                {
                    case "offence":
                        newCiv.Offence = amt;
                        break;

                    case "defence":
                        newCiv.Defence = amt;
                        break;

                    case "research":
                        newCiv.Research = amt;
                        break;

                    case "education":
                        newCiv.Education = amt;
                        break;

                    case "healthcare":
                        newCiv.Healthcare = amt;
                        break;
                }

                amt /= 2;

                // Final iteration, save everything
                if (i == 2)
                {
                    CivManager.Instance.Civs.Add(userHash, newCiv);

                    await CivManager.Instance.Save();
                    await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                    .WithContent($"{String.Join(" ", name)} has been registered as a nation with id {userHash}"));
                    break;
                }
                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                .WithContent("Choose your main stats:")
                .AddComponents(new DiscordSelectComponent("stat", "Select...", options)));
            }
        }

        [Command("create")]
        public async Task CreateCiv(CommandContext context)
        {
            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (UserExists(userHash))
            {
                await context.RespondAsync("You've already registered.");
                return;
            }
            await context.RespondAsync("You must specify a name for your nation.");
        }

        [Command("allocate")]
        public async Task ChangeStats(CommandContext context, string stat, int change)
        {
            CivManager.Instance.CheckForUpdates();

            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (!UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = CivManager.Instance.Civs[userHash];
            if (change == 0)
            {
                await context.RespondAsync("Congrats, you changed a stat by 0.");
                return;
            }

            int p;
            switch (stat.ToLower())
            {
                case "offence":
                case "offense":
                    p = computePointDifference(userCiv.Offence, userCiv.Offence + change);
                    if (change > 0)
                    {
                        if (p <= userCiv.Points)
                        {
                            userCiv.Offence += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough points to increase your offence by {maxPointDifference(userCiv.Offence, userCiv.Points)} level(s).");
                        }

                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Offence += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough point respecs to decrease your offence by {-maxPointDifference(userCiv.Offence, -userCiv.Respec)} level(s).");
                        }
                    }
                    break;

                case "defence":
                case "defense":
                    p = computePointDifference(userCiv.Defence, userCiv.Defence + change);
                    if (change > 0)
                    {
                        if (p <= userCiv.Points)
                        {
                            userCiv.Defence += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough points to increase your defence by {maxPointDifference(userCiv.Defence, userCiv.Points)} level(s).");
                        }
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Defence += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough point respecs to decrease your defence by {maxPointDifference(userCiv.Defence, userCiv.Points)} level(s).");
                        }
                    }
                    break;

                case "research":
                    p = computePointDifference(userCiv.Research, userCiv.Research + change);
                    if (change > 0)
                    {
                        if (p <= userCiv.Points)
                        {
                            userCiv.Research += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough points to increase your research by {maxPointDifference(userCiv.Research, userCiv.Points)} level(s).");
                        }
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Research += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough point respecs to decrease your research by {maxPointDifference(userCiv.Research, userCiv.Points)} level(s).");
                        }
                    }
                    break;

                case "education":
                    p = computePointDifference(userCiv.Education, userCiv.Education + change);
                    if (change > 0)
                    {
                        if (p <= userCiv.Points)
                        {
                            userCiv.Education += p;
                            userCiv.Points += p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough points to increase your education by {maxPointDifference(userCiv.Education, userCiv.Points)} level(s).");
                        }
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Education += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough point respecs to decrease your education by {maxPointDifference(userCiv.Education, userCiv.Points)} level(s).");
                        }
                    }
                    break;

                case "healthcare":
                    p = computePointDifference(userCiv.Healthcare, userCiv.Healthcare + change);
                    if (change > 0)
                    {
                        if (p <= userCiv.Points)
                        {
                            userCiv.Healthcare += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough points to increase your healthcare by {maxPointDifference(userCiv.Healthcare, userCiv.Points)} level(s).");
                        }
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Healthcare += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
                        }
                        else
                        {
                            await context.RespondAsync($"You only have enough point respecs to decrease your healthcare by {maxPointDifference(userCiv.Healthcare, userCiv.Points)} level(s).");
                        }
                    }
                    break;

                default:
                    await context.RespondAsync("That's not a valid stat.");
                    return;
            }
            await CivManager.Instance.Save();
            await ShowStats(context);
        }

        [Command("stats"), Aliases("show")]
        public async Task ShowStats(CommandContext context)
        {
            CivManager.Instance.CheckForUpdates();

            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (!UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = CivManager.Instance.Civs[userHash];

            await context.RespondAsync(userCiv.Format());
        }
    }
}