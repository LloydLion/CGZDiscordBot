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
				StringPrefix = "/",
				EnableMentionPrefix = false,
				EnableDms = false,
				EnableDefaultHelp = true,
			};


			client.UseCommandsNext(config);
			client.GetCommandsNext().RegisterCommands<CommandHandler>();

			ServerDefine.Init(client);

			client.ConnectAsync();

			Thread.Sleep(-1);
		}
	}
}
