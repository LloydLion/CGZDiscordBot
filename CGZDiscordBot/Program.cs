using Discord;
using Discord.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new DiscordSocketClient();

			client.Log += (m) => { Console.WriteLine(m.Message); return Task.CompletedTask; };
			client.MessageReceived += Client_MessageReceived;

			client.LoginAsync(TokenType.Bot, @"NzgyMzQ0MDA1OTUzMzg4NTg0.X8K0og.xnmh5esDi21KrQRPiN1IQkYc2Wk").ContinueWith((s) => client.StartAsync());
			Thread.Sleep(-1);
		}

		private static Task Client_MessageReceived(SocketMessage arg)
		{


			return Task.CompletedTask;
		}
	}
}
