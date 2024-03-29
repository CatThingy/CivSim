using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace CivSim
{
    public class CivStatConverter : IArgumentConverter<Stat>
    {
        public Task<Optional<Stat>> ConvertAsync(string value, CommandContext context)
        {
            switch (value.ToLower())
            {
                case "o":
                case "offence":
                case "offense":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Offence));
                case "d":
                case "defence":
                case "defense":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Defence));
                case "r":
                case "research":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Research));
                case "e":
                case "education":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Education));
                case "h":
                case "healthcare":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Healthcare));
                case "c":
                case "civilian":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Civilian));
                case "m":
                case "morale":
                    return Task.FromResult(Optional.FromValue<Stat>(Stat.Morale));
                default:
                    return Task.FromResult(Optional.FromNoValue<Stat>());
            }
        }
    }

    public class CivManagementModule : BaseCommandModule
    {
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
        int maxPointDifference(int from, int points)
        {
            int k1 = (int)Math.Ceiling((-5 + Math.Sqrt(26 + 8 * Math.Abs(from + points))) / 10d);
            int k2 = (int)Math.Ceiling((-5 + Math.Sqrt(26 + 8 * Math.Abs(from))) / 10d);
            return (int)(Math.Sign(points) * ((((Math.Abs(from + points)) / k1) + 12.5 * (k1 - 1)) - Math.Abs(from) / k2 + 12.5 * (k2 - 1)));
        }

        string AddSign(int num) =>
            num >= 0 ? "+" + num : num.ToString();


        DiscordEmbed FormatCiv(Civ c)
        {
            var e = new DiscordEmbedBuilder()
            .WithTitle(c.Name)
            .WithDescription($"Unspent points: {c.Points}\n Respecs available: {c.Respec}\n\n\u200b")
            .WithFooter($"Nation ID: {c.Id}")
            .WithThumbnail(c.Flag)
            .AddField("Offence: " + c.Stats[Stat.Offence] + $" ({AddSign(c.Effects[Stat.Offence])})", "Attack modifier: " + c.AttackMod, true)
            .AddField("Defence: " + c.Stats[Stat.Defence] + $" ({AddSign(c.Effects[Stat.Defence])})", "Defence modifier: " + c.DefenceMod, true)
            .AddField("\u200b", "\u200b")
            .AddField("Research: " + c.Stats[Stat.Research] + $" ({AddSign(c.Effects[Stat.Research])})", "Weekly points: " + c.WeeklyPoints, true)
            .AddField("Education: " + c.Stats[Stat.Education] + $" ({AddSign(c.Effects[Stat.Education])})", "Weekly respecs: " + c.WeeklyRespecs, true)
            .AddField("\u200b", "\u200b")
            .AddField("Healthcare: " + c.Stats[Stat.Healthcare] + $" ({AddSign(c.Effects[Stat.Healthcare] - (4 * (c.Events.Count - 1)))})", "Event effect modifier: " + -c.EventPenaltyMod, true)
            .AddField("Civilian: " + c.Stats[Stat.Civilian] + $" ({AddSign(c.Effects[Stat.Civilian])})", "\u200b", true)
            .AddField("\u200b", "\u200b")
            .AddField("Morale: " + c.Stats[Stat.Morale] + $" ({AddSign(c.Effects[Stat.Morale])})", "\u200b", true);

            if (c.Colour != null)
            {
                e.WithColor(new DiscordColor(c.Colour));
            }
            return e.Build();
        }

        List<DiscordSelectComponentOption> updateNumbers(List<DiscordSelectComponentOption> options, int num)
        {
            var temp = new List<DiscordSelectComponentOption>();
            // Update the numbers
            foreach (DiscordSelectComponentOption option in options)
            {
                var newOption = new DiscordSelectComponentOption($"{AddSign(num)} to {option.Value}", option.Value);
                temp.Add(newOption);
            }
            return temp;
        }

        //TODO: Replace this whole thing with a slash command so that other people can't make the mistake of interacting with it
        [Command("create")]
        [Aliases("register")]
        [Description("Creates your nation, allowing you to use the rest of the bot.")]
        public async Task CreateCiv(CommandContext context, [Description("The name of your nation.")] params string[] name)
        {
            string userHash = SimManager.GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (SimManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You've already registered.");
                return;
            }
            string joinedName = String.Join(" ", name);
            if (joinedName == "")
            {
                await context.RespondAsync("You must specify a name for your nation.");
                return;
            }
            Civ newCiv = new Civ(String.Join(" ", name), userHash);

            // Set this up to remove options more easily
            var stats = new Dictionary<string, DiscordSelectComponentOption>()
            {
                {"offence", new DiscordSelectComponentOption("Offence", "offence")},
                {"defence", new DiscordSelectComponentOption("Defence", "defence")},
                {"research", new DiscordSelectComponentOption("Research", "research")},
                {"education", new DiscordSelectComponentOption("Education", "education")},
                {"healthcare", new DiscordSelectComponentOption("Healthcare", "healthcare")},
                {"civilian", new DiscordSelectComponentOption("Civilian", "civilian")},
                {"morale", new DiscordSelectComponentOption("Morale", "morale")},
            };

            var options = new List<DiscordSelectComponentOption>()
            {
                stats["offence"],
                stats["defence"],
                stats["research"],
                stats["education"],
                stats["healthcare"],
                stats["civilian"],
                stats["morale"]
            };

            int amt = 16;

            // Update the numbers
            options = updateNumbers(options, amt);
            foreach (DiscordSelectComponentOption option in options)
            {
                stats[option.Value] = option;
            }

            var statMessage = new DiscordMessageBuilder()
            .WithContent($"Customize your nation:")
            .AddComponents(new DiscordSelectComponent("stat", "Select...", options));

            DiscordMessage msg = await context.RespondAsync(statMessage);

            var result = new InteractivityResult<ComponentInteractionCreateEventArgs>();


            // +16/+8/+4 initial stats
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
                        newCiv.Stats[Stat.Offence] = amt;
                        break;

                    case "defence":
                        newCiv.Stats[Stat.Defence] = amt;
                        break;

                    case "research":
                        newCiv.Stats[Stat.Research] = amt;
                        break;

                    case "education":
                        newCiv.Stats[Stat.Education] = amt;
                        break;

                    case "healthcare":
                        newCiv.Stats[Stat.Healthcare] = amt;
                        break;
                    case "civilian":
                        newCiv.Stats[Stat.Civilian] = amt;
                        break;
                    case "morale":
                        newCiv.Stats[Stat.Morale] = amt;
                        break;
                }

                amt /= 2;

                // Update the numbers
                options = updateNumbers(options, amt);
                foreach (DiscordSelectComponentOption option in options)
                {
                    stats[option.Value] = option;
                }


                // Final iteration
                if (i == 2)
                {
                    break;
                }
                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                .WithContent("Customize your nation:")
                .AddComponents(new DiscordSelectComponent("stat", "Select...", options)));
            }

            amt = -16;

            // Update the numbers
            options = updateNumbers(options, amt);
            foreach (DiscordSelectComponentOption option in options)
            {
                stats[option.Value] = option;
            }

            await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
            .WithContent("Customize your nation:")
            .AddComponents(new DiscordSelectComponent("stat", "Select...", options)));

            // -16/-8/-4 initial stats
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
                        newCiv.Stats[Stat.Offence] = amt;
                        break;

                    case "defence":
                        newCiv.Stats[Stat.Defence] = amt;
                        break;

                    case "research":
                        newCiv.Stats[Stat.Research] = amt;
                        break;

                    case "education":
                        newCiv.Stats[Stat.Education] = amt;
                        break;

                    case "healthcare":
                        newCiv.Stats[Stat.Healthcare] = amt;
                        break;
                    case "civilian":
                        newCiv.Stats[Stat.Civilian] = amt;
                        break;
                    case "morale":
                        newCiv.Stats[Stat.Morale] = amt;
                        break;
                }

                amt /= 2;

                // Update the numbers
                options = updateNumbers(options, amt);
                foreach (DiscordSelectComponentOption option in options)
                {
                    stats[option.Value] = option;
                }

                // Final iteration, save everything
                if (i == 2)
                {
                    SimManager.Instance.Civs.Add(userHash, newCiv);

                    await SimManager.Instance.Save();
                    await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                    .WithContent("")
                    .AddEmbed(FormatCiv(newCiv)));
                    break;
                }
                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                .WithContent("Customize your nation:")
                .AddComponents(new DiscordSelectComponent("stat", "Select...", options)));
            }
        }

        [Command("allocate")]
        [Description("Allocates points to increase your stats.\n\nStats cost 1 point more every 25 levels.")]
        public async Task ChangeStats(CommandContext context,
            [Description("The stat to allocate points to.")]
            Stat stat,

            [Description("The number of levels to change the stat by.\nIf negative, uses point respecs and refunds points.")]
            int change)
        {
            SimManager.Instance.CheckForUpdates();

            string userHash = SimManager.GetUserHash(context.User.Id);
            if (!SimManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = SimManager.Instance.Civs[userHash];
            userCiv.UpdateEvents();
            if (change == 0)
            {
                await context.RespondAsync("Congrats, you changed a stat by 0.");
                return;
            }

            int p = computePointDifference(userCiv.Stats[stat], userCiv.Stats[stat] + change);
            if (change > 0)
            {
                if (p <= userCiv.Points)
                {
                    userCiv.Stats[stat] += p;
                    userCiv.Points -= p;
                }
                else
                {
                    await context.RespondAsync($"You only have enough points to increase your offence by {maxPointDifference(userCiv.Stats[stat], userCiv.Stats[stat])} level(s).");
                }

            }
            else
            {
                if (-p <= userCiv.Respec)
                {
                    userCiv.Stats[stat] += p;
                    userCiv.Respec += p;
                    userCiv.Points -= p;
                }
                else
                {
                    await context.RespondAsync($"You only have enough point respecs to decrease your offence by {-maxPointDifference(userCiv.Stats[stat], -userCiv.Respec)} level(s).");
                }
            }
            await SimManager.Instance.Save();
            await ShowStats(context);
        }

        [Command("stats")]
        [Aliases("show")]
        [Description("Shows stats of a nation.")]
        public async Task ShowStats(CommandContext context,
            [Description("The ID of the nation to show.\nDefaults to your nation.")]
            string id = "")
        {
            SimManager.Instance.CheckForUpdates();

            string userHash = SimManager.GetUserHash(context.User.Id);
            if (id == "")
            {
                id = userHash;
            }

            if (!SimManager.Instance.UserExists(id))
            {
                if (id == userHash)
                {
                    await context.RespondAsync("You haven't registered yet.");
                }
                else
                {
                    await context.RespondAsync("That nation doesn't exist.");
                }
                return;
            }

            Civ userCiv = SimManager.Instance.Civs[id];
            userCiv.UpdateEvents();

            await context.RespondAsync(FormatCiv(userCiv));
        }

        [Command("flag")]
        public async Task SetFlag(CommandContext context)
        {
            string userHash = SimManager.GetUserHash(context.User.Id);
            if (!SimManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = SimManager.Instance.Civs[userHash];


            if (context.Message.Attachments.Count == 0)
            {
                await context.RespondAsync("Attach an image to set it as your flag.");
                return;
            }

            DiscordAttachment flag = context.Message.Attachments[0];

            if (flag.Width == null)
            {
                await context.RespondAsync("You can only set images as flags.");
                return;
            }

            userCiv.Flag = flag.Url;

            await SimManager.Instance.Save();
            await ShowStats(context, userHash);
        }

        [Command("name")]
        public async Task SetName(CommandContext context, params string[] name)
        {
            string userHash = SimManager.GetUserHash(context.User.Id);
            if (!SimManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = SimManager.Instance.Civs[userHash];

            userCiv.Name = String.Join(" ", name);

            await SimManager.Instance.Save();
            await ShowStats(context, userHash);
        }

        [Command("colour")]
        [Aliases("color")]
        public async Task SetColour(CommandContext context, DiscordColor colour)
        {
            string userHash = SimManager.GetUserHash(context.User.Id);
            if (!SimManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = SimManager.Instance.Civs[userHash];

            userCiv.Colour = colour.ToString();

            await SimManager.Instance.Save();
            await ShowStats(context, userHash);
        }

        [Command("wipe")]
        [Aliases("nuke")]
        [RequireUserPermissions(Permissions.KickMembers)]
        public async Task WipeCiv(CommandContext context, string target)
        {
            if (SimManager.Instance.UserExists(target))
            {
                SimManager.Instance.Civs.Remove(target);
                await SimManager.Instance.Save();
                await context.RespondAsync("Nation deleted.");
            }
            else
            {
                await context.RespondAsync("Nation could not be found.");
            }
        }

        [Command("delete")]
        public async Task Delete(CommandContext context)
        {
            string userHash = SimManager.GetUserHash(context.User.Id);
            if (SimManager.Instance.UserExists(userHash))
            {
                SimManager.Instance.Civs.Remove(userHash);
                await SimManager.Instance.Save();
                await context.RespondAsync("Nation deleted.");
            }
            else
            {
                await context.RespondAsync("You don't have a nation to delete.");
            }
        }
    }
}