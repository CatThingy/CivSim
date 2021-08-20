using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Converters;

namespace CivSim
{
    public class HelpFormatter : DefaultHelpFormatter
    {
        private CommandContext ctx;

        public HelpFormatter(CommandContext context) : base(context)
        {
            ctx = context;
        }

        public override CommandHelpMessage Build()
        {
            string userHash = SimManager.GetUserHash(ctx.User.Id);
            if (!SimManager.Instance.UserExists(userHash))
            {
                EmbedBuilder.ClearFields()
                .WithTitle("Getting Started")
                .WithDescription("Create a nation with `c!create [name]` to access the rest of the bot.");

                return new CommandHelpMessage(embed: EmbedBuilder.Build());
            }
            return base.Build();
        }
    }
}