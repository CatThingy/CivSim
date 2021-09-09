using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace CivSim
{
    public class WarModule : BaseCommandModule
    {
        public async Task DeclareWar(CommandContext context, string target)
        {

            string userHash = SimManager.GetUserHash(context.User.Id);

            if (!SimManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }

            Civ userCiv = SimManager.Instance.Civs[userHash];

            if (!SimManager.Instance.UserExists(target))
            {
                await context.RespondAsync($"That nation doesn't exist.");
                return;
            }

            Civ targetCiv = SimManager.Instance.Civs[target];

            foreach (War w in SimManager.Instance.Wars.Values)
            {
                if ((w.A.Contains(userHash) && w.B.Contains(target)) || (w.B.Contains(userHash) && w.A.Contains(target)))
                {
                    await context.RespondAsync($"You're already at war with that nation.");
                    return;
                }
            }

            War newWar = new War();
            newWar.A.Add(userHash);
            newWar.B.Add(target);

            string id = SimManager.GetUserHash((ulong)DateTime.Now.ToBinary());

            SimManager.Instance.Wars.Add(id, newWar);
        }
    }
}