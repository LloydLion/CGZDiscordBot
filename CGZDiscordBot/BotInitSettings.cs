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

		public DiscordChannel VoiceChannelCreationChannel { get; set; }

		public DiscordChannel VoiceChannelCategory { get; set; }
	}
}
