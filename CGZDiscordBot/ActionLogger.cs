using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class ActionLogger
	{
		public string LogPath { get; }


		public enum ActionType
		{
			WriteMessage,
			EditMessage,
			DeleteMessage,
			AddReaction,
			RemoveReaction,
		}


		public ActionLogger(DiscordClient discordClient, string logPath)
		{
			LogPath = logPath;


			discordClient.MessageCreated += (s, e) =>
			{ Task.Run(() => WriteToLog(ActionType.WriteMessage, e.Guild, e.Channel, e.Guild.GetMemberAsync(e.Author.Id).Result, e.Message.Id + "|  " + e.Message.Content)); return Task.CompletedTask; };

			discordClient.MessageDeleted += (s, e) =>
			{ Task.Run(() => WriteToLog(ActionType.DeleteMessage, e.Guild, e.Channel, null, e.Message.Id.ToString())); return Task.CompletedTask; };

			discordClient.MessageUpdated += (s, e) =>
			{ Task.Run(() => WriteToLog(ActionType.EditMessage, e.Guild, e.Channel, e.Guild.GetMemberAsync(e.Author.Id).Result, e.Message.Id + "|  " + e.Message.Content)); return Task.CompletedTask; };


			discordClient.MessageReactionAdded += (s, e) =>
			{ Task.Run(() => WriteToLog(ActionType.AddReaction, e.Guild, e.Channel, e.Guild.GetMemberAsync(e.User.Id).Result, e.Message.Id + "|  " + e.Emoji.Name)); return Task.CompletedTask; };

			discordClient.MessageReactionRemoved += (s, e) =>
			{ Task.Run(() => WriteToLog(ActionType.RemoveReaction, e.Guild, e.Channel, e.Guild.GetMemberAsync(e.User.Id).Result, e.Message.Id + "|  " + e.Emoji.Name)); return Task.CompletedTask; };
		}


		#nullable enable
		public void WriteToLog(ActionType action, DiscordGuild guild, DiscordChannel channel, DiscordMember? member, string data = "")
		{
			var file = File.Open(LogPath + "\\" + guild.Id + ".log", FileMode.Append, FileAccess.Write);

			file.Write(Encoding.UTF8.GetBytes($"[{DateTime.Now}] {action} by @{member?.DisplayName}[{member?.Username}:{member?.Discriminator}-{member?.Id}] on #{channel.Name}[{channel.Id}], data[{data}]\r\n"));
			file.Flush();
			file.Close();
		}
	}
}
