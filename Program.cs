using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace CivSim
{
    class Program
    {
        public struct Config
        {
            public string Token { get; set; }
            public string Prefix { get; set; }
        }

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            string configString = File.ReadAllText("config.json");
            Config config = JsonSerializer.Deserialize<Config>(configString);

            DiscordClient Discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.DirectMessages
                | DiscordIntents.GuildMessages
                | DiscordIntents.Guilds
            });

            CommandsNextExtension commands = Discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { config.Prefix }
            });

            commands.RegisterCommands<CivManagementModule>();

            await Discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}

