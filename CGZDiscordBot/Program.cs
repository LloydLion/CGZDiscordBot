using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using StandardLibrary.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class Program
	{
		public static DataSaver SettingsSaver { get; } = new DataSaver(DataSaver.DataLocation.ProgramDirectory);


		static void Main(string[] args)
		{
			DataSaver.SetApplicationName("CGZDiscordBot");
			if (SettingsSaver.HasKey("init")) BotInitSettings.ServersData.AddRange(SettingsSaver.GetSavedObject<Dictionary<ulong, BotInitSettings>>("init"));

			var client = new DiscordClient(new DiscordConfiguration { TokenType = TokenType.Bot, Token = @"NzgyMzQ0MDA1OTUzMzg4NTg0.X8K0og.xnmh5esDi21KrQRPiN1IQkYc2Wk" });

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

			client.UseCommandsNext(commConfig);
			client.UseInteractivity(interactConfig);

			client.GetCommandsNext().RegisterCommands<CommandHandler>();

			client.ConnectAsync().Wait();

			Thread.Sleep(-1);
		}
	}
}
