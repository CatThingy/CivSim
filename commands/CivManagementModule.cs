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

    }
}