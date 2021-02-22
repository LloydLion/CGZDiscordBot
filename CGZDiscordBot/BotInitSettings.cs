using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZDiscordBot
{
	class BotInitSettings
	{
		public static Dictionary<ulong, BotInitSettings> ServersData { get; } = new Dictionary<ulong, BotInitSettings>();


		public ulong MusicInfoChannel { get; set; }

		public ulong VoiceChannelCategory { get; set; }

		public ulong DefaultMemberRole { get; set; }

		public ulong AnnountmentsChannel { get; set; }

		public ulong StreamSubscriberRole { get; set; }

		public ulong Administrator { get; set; }

		public ulong MutedMemberRole { get; set; }

		public ulong TeamFindingChannel { get; set; }

		public ulong UncensorChannel { get; set; }

		public ulong MusicChannel { get; set; }


		public static DiscordChannel GetMusicInfoChannel(DiscordGuild guild) => guild.GetChannel(ServersData[guild.Id].MusicInfoChannel);

		public static DiscordChannel GetVoiceChannelCategory(DiscordGuild guild) => guild.GetChannel(ServersData[guild.Id].VoiceChannelCategory);

		public static DiscordRole GetDefaultMemberRole(DiscordGuild guild) => guild.GetRole(ServersData[guild.Id].DefaultMemberRole);

		public static DiscordChannel GetAnnountmentsChannel(DiscordGuild guild) => guild.GetChannel(ServersData[guild.Id].AnnountmentsChannel);

		public static DiscordRole GetStreamSubscriberRole(DiscordGuild guild) => guild.GetRole(ServersData[guild.Id].StreamSubscriberRole);

		public static DiscordMember GetAdministrator(DiscordGuild guild) => guild.GetMemberAsync(ServersData[guild.Id].Administrator).Result;

		public static DiscordRole GetMutedMemberRole(DiscordGuild guild) => guild.GetRole(ServersData[guild.Id].MutedMemberRole);

		public static DiscordChannel GetTeamFindingChannel(DiscordGuild guild) => guild.GetChannel(ServersData[guild.Id].TeamFindingChannel);

		public static DiscordChannel GetUncensorChannel(DiscordGuild guild) => guild.GetChannel(ServersData[guild.Id].UncensorChannel);

		public static DiscordChannel GetMusicChannel(DiscordGuild guild) => guild.GetChannel(ServersData[guild.Id].MusicChannel);
	}
}
