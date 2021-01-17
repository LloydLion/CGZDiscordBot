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


		public ulong VoiceChannelCreationChannel { get; set; }

		public ulong VoiceChannelCategory { get; set; }

		public ulong DefaultMemberRole { get; set; }

		public ulong AnnountmentsChannel { get; set; }

		public ulong StreamSubscriberRole { get; set; }

		public ulong Administrator { get; set; }

		public ulong MutedMemberRole { get; set; }


		public DiscordChannel GetVoiceChannelCreationChannel(DiscordGuild guild) => guild.GetChannel(VoiceChannelCreationChannel);

		public DiscordChannel GetVoiceChannelCategory(DiscordGuild guild) => guild.GetChannel(VoiceChannelCategory);

		public DiscordRole GetDefaultMemberRole(DiscordGuild guild) => guild.GetRole(DefaultMemberRole);

		public DiscordChannel GetAnnountmentsChannel(DiscordGuild guild) => guild.GetChannel(AnnountmentsChannel);

		public DiscordRole GetStreamSubscriberRole(DiscordGuild guild) => guild.GetRole(StreamSubscriberRole);

		public DiscordMember GetAdministrator(DiscordGuild guild) => guild.GetMemberAsync(Administrator).Result;

		public DiscordRole GetMutedMemberRole(DiscordGuild guild) => guild.GetRole(MutedMemberRole);
	}
}
