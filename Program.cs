using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Rest;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using HeavenholdBot.Models;
using HeavenholdBot.Repositories;
using System.Configuration;
using Interactivity;
using MySqlConnector;
using HeavenholdBot;

await new Program().RunBotAsync();

namespace HeavenholdBot
{
    public class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private InteractivityService _interactive;
        public static HeroRepository _heroList;
        public static ItemRepository _itemList;

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                MessageCacheSize = 10000,

                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages |
                         GatewayIntents.GuildMessageReactions | GatewayIntents.GuildPresences | GatewayIntents.GuildWebhooks | GatewayIntents.GuildMembers | GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessages | GatewayIntents.GuildEmojis,

                LogLevel = LogSeverity.Info
            });
            _commands = new CommandService();

            _heroList = new HeroRepository();
            _itemList = new ItemRepository();

            InteractivityConfig _config = new InteractivityConfig();
            _config.DefaultTimeout = TimeSpan.FromMinutes(3);
            _config.RunOnGateway = true;
            _interactive = new InteractivityService(_client, _config);

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_interactive)
                .BuildServiceProvider();

            string token = System.Configuration.ConfigurationManager.AppSettings["discordBotToken"];

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.SetGameAsync("Guardian Tales", null, ActivityType.Playing);

            await _client.StartAsync();

            await Task.Delay(-1);

        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleMessages;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleMessages(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if(message is null) return;

            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }
    }
}
