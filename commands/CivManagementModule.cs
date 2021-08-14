using System;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

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
            return CivManager.Civs.ContainsKey(userHash);
        }

        // Levels 0-24: costs 1 point
        // Levels 25-49: costs 2 points
        // Levels 50-74: costs 3 points
        // etc.
        int computePointDifference(int from, int to)
        {
            int l1 = (int)Math.Floor(Math.Abs(from) / 25d);
            int l2 = (int)Math.Floor(Math.Abs(to) / 25d);

            return Math.Sign(to) * (((l2 * (l2 + 1)) / 2) * 25 + (Math.Abs(to) % 25) * (l2 + 1)) - Math.Sign(from) * (((l1 * (l1 + 1)) / 2) * 25 + (Math.Abs(from) % 25) * (l1 + 1));
        }

        [Command("create"), Aliases("register")]
        public async Task CreateCiv(CommandContext context, string name)
        {
            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (UserExists(userHash))
            {
                await context.RespondAsync("You've already registered.");
                return;
            }
            Civ newCiv = new Civ(name);
            CivManager.Civs.Add(userHash, newCiv);

            await CivManager.SaveCivs();
            await context.RespondAsync($"{name} has been registered as a nation with an ID of {userHash}");
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

        [Command("stats")]
        public async Task ChangeStats(CommandContext context, string stat, int change)
        {
            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (!UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = CivManager.Civs[userHash];
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
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Offence += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
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
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Defence += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
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
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Research += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
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
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Education += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
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
                    }
                    else
                    {
                        if (-p <= userCiv.Respec)
                        {
                            userCiv.Healthcare += p;
                            userCiv.Respec += p;
                            userCiv.Points -= p;
                        }
                    }
                    break;
                default:
                    await context.RespondAsync("That's not a recognized stat.");
                    return;
            }
        }

        [Command("stats")]
        public async Task ChangeStats(CommandContext context)
        {
            string userHash = GetUserHash(((context.Member as SnowflakeObject ?? context.Guild as SnowflakeObject).Id));
            if (!UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = CivManager.Civs[userHash];

            await context.RespondAsync($"O:{userCiv.Offence}\nD:{userCiv.Defence}\nR:{userCiv.Research}\nE:{userCiv.Education}\nH:{userCiv.Healthcare}");
        }
    }
}