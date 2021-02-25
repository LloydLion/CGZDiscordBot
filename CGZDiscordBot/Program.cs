using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
//using DSharpPlus.VoiceNext;
using StandardLibrary.Data;
using StandardLibrary.Other;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class Program
	{
		public static SettingsSaver SettingsSaver { get; } = new SettingsSaver();


		static void Main(string[] args)
		{
			SettingsSaver.ConcatFromDefault();
			BotInitSettings.ServersData.AddRange(SettingsSaver.SettingsDictinary);

			var client = new DiscordClient(new DiscordConfiguration { TokenType = TokenType.Bot, Token = File.ReadAllText("token.ini") });

			CommandsNextConfiguration commConfig = new CommandsNextConfiguration
			{
				StringPrefixes = new string[] { "/" },
				EnableMentionPrefix = false,
				EnableDms = false,
				EnableDefaultHelp = true,
			};

			InteractivityConfiguration interactConfig = new InteractivityConfiguration
			{

			};

			//VoiceNextConfiguration voiceConfig = new VoiceNextConfiguration
			//{
				 
			//};

			client.MessageCreated += (sender, s) => { Task.Run(() => CensorChat(s.Message, s.Guild)); return Task.CompletedTask; };
			client.MessageUpdated += (sender, s) => { Task.Run(() => CensorChat(s.Message, s.Guild)); return Task.CompletedTask; };
			//client.GuildDownloadCompleted += (sender, s) =>
			//{
			//	s.Guilds.Values.Where(s => BotInitSettings.ServersData.ContainsKey(s.Id))
			//		.InvokeForAll(g => sender.GetVoiceNext().ConnectAsync(BotInitSettings.GetMusicChannel(g)));

			//	return Task.CompletedTask;
			//};

			//Log System
			var logger = new ActionLogger(client, "logs");

			client.UseCommandsNext(commConfig);
			client.UseInteractivity(interactConfig);
			//client.UseVoiceNext(voiceConfig);

			client.GetCommandsNext().RegisterCommands<CommandHandler>();

			client.ConnectAsync().Wait();

			Thread.Sleep(-1);
		}


		public static void CensorChat(DiscordMessage msg, DiscordGuild guild)
		{
			var censorWords = File.ReadAllText(".\\CensorWords.txt").Split("\r\n");

			if(BotInitSettings.ServersData.ContainsKey(guild.Id)/*init check*/ 
				&& msg.Channel != BotInitSettings.GetUncensorChannel(guild))
			{

				var content = msg.Content.ToLower();

				if(content.Split(' ', '-', '_', '&', '(', ')').ContainsAnyElementOf(censorWords))
				{
					msg.DeleteAsync();

					var member = guild.GetMemberAsync(msg.Author.Id).Result;

					msg.Channel
						.SendMessageAsync(member.Mention + " ваше сообщение было удалено по причине ислользования нецензурной лексики.\r\n Вы были заглушены на 02:00:00");

					member.RevokeRoleAsync(BotInitSettings.GetDefaultMemberRole(guild));
					member.GrantRoleAsync(BotInitSettings.GetMutedMemberRole(guild));

					Task.Run(async () =>
					{
						var span = new TimeSpan(2, 0, 0);
						await Task.Delay((int)span.TotalMilliseconds);

						await member.GrantRoleAsync(BotInitSettings.GetDefaultMemberRole(guild)).ThrowTaskException();
						await member.RevokeRoleAsync(BotInitSettings.GetMutedMemberRole(guild)).ThrowTaskException();

						await msg.Channel.SendMessageAsync(member.Mention + " теперь может говорить(и писать тоже)!");
					});
				}
			}
		}
	}
}
