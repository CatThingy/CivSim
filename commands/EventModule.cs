using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace CivSim
{
    public class EventModule : BaseCommandModule
    {
        Random rnd = new Random(DateTime.Now.Millisecond);

        List<DiscordSelectComponentOption> EventOptions = new List<DiscordSelectComponentOption>()
        {
            new DiscordSelectComponentOption("-1 Offence", "evt_offence1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Offence", "evt_offence2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Offence", "evt_offence3", "Costs 2 point to exclude", true),

            new DiscordSelectComponentOption("-1 Defence", "evt_defence1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Defence", "evt_defence2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Defence", "evt_defence3", "Costs 2 point to exclude", true),

            new DiscordSelectComponentOption("-1 Research", "evt_research1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Research", "evt_research2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Research", "evt_research3", "Costs 2 point to exclude", true),

            new DiscordSelectComponentOption("-1 Education", "evt_education1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Education", "evt_education2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Education", "evt_education3", "Costs 2 point to exclude", true),

            new DiscordSelectComponentOption("-1 Healthcare", "evt_healthcare1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Healthcare", "evt_healthcare2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Healthcare", "evt_healthcare3", "Costs 2 point to exclude", true),

            new DiscordSelectComponentOption("-1 Civilian", "evt_civilian1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Civilian", "evt_civilian2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Civilian", "evt_civilian3", "Costs 2 point to exclude", true),

            new DiscordSelectComponentOption("-1 Morale", "evt_morale1", "Costs 1 points to exclude", true),
            new DiscordSelectComponentOption("-2 Morale", "evt_morale2", "Costs 2 points to exclude", true),
            new DiscordSelectComponentOption("-4 Morale", "evt_morale3", "Costs 2 point to exclude", true),
        };

        //TODO: anon. events
        [Command("event")]
        public async Task CreateEvent(CommandContext context, string target)
        {
            string userHash = CivManager.GetUserHash(context.User.Id);
            if (!CivManager.Instance.UserExists(userHash))
            {
                await context.RespondAsync("You haven't registered yet.");
                return;
            }
            if (!CivManager.Instance.UserExists(target))
            {
                await context.RespondAsync($"That nation doesn't exist.");
                return;
            }

            Civ userCiv = CivManager.Instance.Civs[userHash];
            Civ targetCiv = CivManager.Instance.Civs[target];

            var dropdown = new DiscordSelectComponent("events", "Choose...", EventOptions, false, 7, 21);
            var cancel = new DiscordButtonComponent(ButtonStyle.Danger, "cancel", "Cancel");
            var confirm = new DiscordButtonComponent(ButtonStyle.Success, "confirm", "Confirm");

            int pointCost = 5;

            DiscordMessage pointsMsg = await context.RespondAsync($"Point cost: {pointCost}\nYour points: {userCiv.Points}");

            DiscordMessage componentMsg = await new DiscordMessageBuilder()
            .WithContent("Choose events to exclude:")
            .AddComponents(dropdown)
            .AddComponents(confirm, cancel)
            .SendAsync(context.Channel);

            var selected = new List<string>(EventOptions.Select(v => v.Value));

            Emzi0767.Utilities.AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> p = async (c, e) =>
                            {
                                // Filter out buttons and other users
                                if (e.Id == "cancel" || e.Id == "confirm") { return; }
                                if (e.User.Id != context.User.Id) { return; }

                                pointCost = 40;

                                selected = new List<string>(e.Values);
                                foreach (string value in e.Values)
                                {
                                    if (value.EndsWith("1"))
                                    {
                                        pointCost -= 1;
                                    }
                                    else
                                    {
                                        pointCost -= 2;
                                    }
                                }
                                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                                await pointsMsg.ModifyAsync($"Point cost: {pointCost}\nYour points: {userCiv.Points}");
                            };

            context.Client.ComponentInteractionCreated += p;

            InteractivityResult<ComponentInteractionCreateEventArgs> result = await componentMsg.WaitForButtonAsync(context.User, null);

            if (!result.TimedOut && result.Result.Id != "cancel")
            {
                var weights = new List<int>();

                foreach (string v in selected)
                {
                    if (v.EndsWith("1"))
                    {
                        weights.Add(1);
                    }
                    else
                    {
                        weights.Add(2);
                    }
                }

                CivEvent civEvent = new CivEvent(CivEvent.ShopEvents[selected[WeightedChoice(weights)]]);
                
                targetCiv.Events.Add(civEvent);
                targetCiv.UpdateEvents();

                await CivManager.Instance.Save();
                await pointsMsg.ModifyAsync(civEvent.FlavourText);
            }

            context.Client.ComponentInteractionCreated -= p;

            // await pointsMsg.DeleteAsync();
            await componentMsg.DeleteAsync();
        }

        int WeightedChoice(List<int> weights)
        {
            int max = weights.Sum();
            int choice = rnd.Next(max);

            for (int i = 0; i < weights.Count; i++)
            {
                if ((choice -= weights[i]) < 0)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}