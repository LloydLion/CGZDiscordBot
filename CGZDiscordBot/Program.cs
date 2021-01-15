using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new DiscordClient(new DiscordConfiguration { TokenType = TokenType.Bot, Token = @"NzgyMzQ0MDA1OTUzMzg4NTg0.X8K0og.xnmh5esDi21KrQRPiN1IQkYc2Wk" });

			CommandsNextConfiguration config = new CommandsNextConfiguration
			{
				StringPrefixes = new string[] { "/" },
				EnableMentionPrefix = false,
				EnableDms = false,
				EnableDefaultHelp = true,
			};


			client.UseCommandsNext(config);
			client.GetCommandsNext().RegisterCommands<CommandHandler>();



			client.ConnectAsync().Wait();

			ServerDefine.Init(client);

			Thread.Sleep(-1);
		}
	}
}
